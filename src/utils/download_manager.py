#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Download manager for handling YouTube video downloads
"""

import os
import time
import json
import threading
from datetime import datetime
from typing import Dict, List, Optional, Tuple, Callable, Any

import yt_dlp
from PySide6.QtCore import QObject, Signal


class DownloadItem:
    """Class representing a download item"""
    
    def __init__(self, url: str, output_path: str, format_id: str, 
                 audio_only: bool = False, embed_subtitles: bool = False,
                 subtitle_lang: str = None, playlist: bool = False):
        self.url = url
        self.output_path = output_path
        self.format_id = format_id
        self.audio_only = audio_only
        self.embed_subtitles = embed_subtitles
        self.subtitle_lang = subtitle_lang
        self.playlist = playlist
        
        # Download status
        self.status = "queued"  # queued, downloading, completed, error, cancelled
        self.progress = 0.0
        self.speed = "0 KiB/s"
        self.eta = "00:00"
        self.title = ""
        self.thumbnail = ""
        self.error_message = ""
        self.download_id = str(int(time.time() * 1000))  # Unique ID based on timestamp
        self.completed_time = None
        self.file_size = 0
        self.downloaded_bytes = 0
        
    def to_dict(self) -> Dict:
        """Convert download item to dictionary for saving to history"""
        return {
            "url": self.url,
            "output_path": self.output_path,
            "format_id": self.format_id,
            "audio_only": self.audio_only,
            "embed_subtitles": self.embed_subtitles,
            "subtitle_lang": self.subtitle_lang,
            "playlist": self.playlist,
            "status": self.status,
            "title": self.title,
            "thumbnail": self.thumbnail,
            "download_id": self.download_id,
            "completed_time": self.completed_time,
            "file_size": self.file_size
        }


class DownloadSignals(QObject):
    """Signals for download progress and status updates"""
    
    progress_updated = Signal(str, float, str, str, int, int)  # download_id, progress, speed, eta, downloaded_bytes, file_size
    status_changed = Signal(str, str)  # download_id, status
    download_started = Signal(str, str)  # download_id, title
    download_finished = Signal(str, str, str)  # download_id, status, error_message
    thumbnail_updated = Signal(str, str)  # download_id, thumbnail_path


class DownloadManager:
    """Manager for handling YouTube video downloads"""
    
    def __init__(self, history_file: str = "download_history.json"):
        self.active_downloads: Dict[str, DownloadItem] = {}
        self.download_threads: Dict[str, threading.Thread] = {}
        self.signals = DownloadSignals()
        self.history_file = history_file
        self.download_history: List[Dict] = self.load_history()
        self.max_concurrent_downloads = 3
        self.download_queue: List[DownloadItem] = []
        self.queue_lock = threading.Lock()
        
    def load_history(self) -> List[Dict]:
        """Load download history from file"""
        if os.path.exists(self.history_file):
            try:
                with open(self.history_file, "r", encoding="utf-8") as f:
                    return json.load(f)
            except (json.JSONDecodeError, IOError):
                return []
        return []
    
    def save_history(self):
        """Save download history to file"""
        try:
            with open(self.history_file, "w", encoding="utf-8") as f:
                json.dump(self.download_history, f, ensure_ascii=False, indent=2)
        except IOError:
            pass
    
    def add_to_history(self, download_item: DownloadItem):
        """Add download item to history"""
        # Set completed time
        if download_item.status in ["completed", "error", "cancelled"]:
            download_item.completed_time = datetime.now().isoformat()
        
        # Add to history
        self.download_history.insert(0, download_item.to_dict())
        
        # Limit history size to 100 items
        if len(self.download_history) > 100:
            self.download_history = self.download_history[:100]
        
        # Save history
        self.save_history()
    
    def clear_history(self):
        """Clear download history"""
        self.download_history = []
        self.save_history()
    
    def add_download(self, download_item: DownloadItem):
        """Add download to queue"""
        with self.queue_lock:
            # Add to queue
            self.download_queue.append(download_item)
            
            # Start download if possible
            self._process_queue()
    
    def _process_queue(self):
        """Process download queue"""
        with self.queue_lock:
            # Check if we can start more downloads
            while len(self.active_downloads) < self.max_concurrent_downloads and self.download_queue:
                # Get next download from queue
                download_item = self.download_queue.pop(0)
                
                # Start download
                self._start_download(download_item)
    
    def _start_download(self, download_item: DownloadItem):
        """Start download in a separate thread"""
        # Add to active downloads
        self.active_downloads[download_item.download_id] = download_item
        
        # Update status
        download_item.status = "downloading"
        self.signals.status_changed.emit(download_item.download_id, "downloading")
        
        # Start download thread
        thread = threading.Thread(
            target=self._download_thread,
            args=(download_item,),
            daemon=True
        )
        self.download_threads[download_item.download_id] = thread
        thread.start()
    
    def _download_thread(self, download_item: DownloadItem):
        """Download thread function"""
        try:
            # Prepare yt-dlp options
            ydl_opts = self._prepare_ydl_opts(download_item)
            
            # Download video
            with yt_dlp.YoutubeDL(ydl_opts) as ydl:
                ydl.download([download_item.url])
            
            # Update status
            download_item.status = "completed"
            self.signals.status_changed.emit(download_item.download_id, "completed")
            self.signals.download_finished.emit(download_item.download_id, "completed", "")
        except Exception as e:
            # Update status
            download_item.status = "error"
            download_item.error_message = str(e)
            self.signals.status_changed.emit(download_item.download_id, "error")
            self.signals.download_finished.emit(download_item.download_id, "error", str(e))
        finally:
            # Add to history
            self.add_to_history(download_item)
            
            # Remove from active downloads
            with self.queue_lock:
                if download_item.download_id in self.active_downloads:
                    del self.active_downloads[download_item.download_id]
                if download_item.download_id in self.download_threads:
                    del self.download_threads[download_item.download_id]
            
            # Process queue
            self._process_queue()
    
    def _prepare_ydl_opts(self, download_item: DownloadItem) -> Dict:
        """Prepare yt-dlp options"""
        ydl_opts = {
            "format": download_item.format_id if not download_item.audio_only else "bestaudio/best",
            "outtmpl": os.path.join(download_item.output_path, "%(title)s.%(ext)s"),
            "progress_hooks": [lambda d: self._progress_hook(d, download_item)],
            "ignoreerrors": True,  # Skip unavailable videos in a playlist
            "noplaylist": not download_item.playlist,  # Download playlist if specified
        }
        
        # Add postprocessors
        postprocessors = []
        
        # Audio only download
        if download_item.audio_only:
            postprocessors.append({
                "key": "FFmpegExtractAudio",
                "preferredcodec": "mp3",
                "preferredquality": "192",
            })
        
        # Embed subtitles
        if download_item.embed_subtitles and download_item.subtitle_lang:
            postprocessors.append({
                "key": "FFmpegEmbedSubtitle",
                "already_have_subtitle": False,
            })
            
            # Add subtitle options
            ydl_opts["writesubtitles"] = True
            ydl_opts["subtitleslangs"] = [download_item.subtitle_lang]
        
        # Add postprocessors to options
        if postprocessors:
            ydl_opts["postprocessors"] = postprocessors
        
        return ydl_opts
    
    def _progress_hook(self, d: Dict, download_item: DownloadItem):
        """Progress hook for yt-dlp"""
        if d["status"] == "downloading":
            # Update progress
            if "total_bytes" in d:
                download_item.file_size = d["total_bytes"]
                download_item.downloaded_bytes = d["downloaded_bytes"]
                download_item.progress = d["downloaded_bytes"] / d["total_bytes"] * 100
            elif "total_bytes_estimate" in d:
                download_item.file_size = d["total_bytes_estimate"]
                download_item.downloaded_bytes = d["downloaded_bytes"]
                download_item.progress = d["downloaded_bytes"] / d["total_bytes_estimate"] * 100
            else:
                download_item.progress = float(d.get("_percent_str", "0%").replace("%", "").strip())
            
            # Update speed and ETA
            download_item.speed = d.get("_speed_str", "0 KiB/s")
            download_item.eta = d.get("_eta_str", "00:00")
            
            # Emit progress signal
            self.signals.progress_updated.emit(
                download_item.download_id,
                download_item.progress,
                download_item.speed,
                download_item.eta,
                download_item.downloaded_bytes,
                download_item.file_size
            )
        elif d["status"] == "finished":
            # Update progress to 100%
            download_item.progress = 100.0
            self.signals.progress_updated.emit(
                download_item.download_id,
                100.0,
                "0 KiB/s",
                "00:00",
                download_item.file_size,
                download_item.file_size
            )
        elif d["status"] == "error":
            # Update status
            download_item.status = "error"
            download_item.error_message = d.get("error", "Unknown error")
            self.signals.status_changed.emit(download_item.download_id, "error")
            self.signals.download_finished.emit(download_item.download_id, "error", d.get("error", "Unknown error"))
        
        # Update title and thumbnail if available
        if "info_dict" in d:
            info_dict = d["info_dict"]
            if "title" in info_dict and not download_item.title:
                download_item.title = info_dict["title"]
                self.signals.download_started.emit(download_item.download_id, info_dict["title"])
            
            if "thumbnail" in info_dict and not download_item.thumbnail:
                download_item.thumbnail = info_dict["thumbnail"]
                self.signals.thumbnail_updated.emit(download_item.download_id, info_dict["thumbnail"])
    
    def cancel_download(self, download_id: str):
        """Cancel download"""
        with self.queue_lock:
            # Check if download is in queue
            for i, item in enumerate(self.download_queue):
                if item.download_id == download_id:
                    # Remove from queue
                    self.download_queue.pop(i)
                    
                    # Update status
                    item.status = "cancelled"
                    self.signals.status_changed.emit(download_id, "cancelled")
                    self.signals.download_finished.emit(download_id, "cancelled", "")
                    
                    # Add to history
                    self.add_to_history(item)
                    
                    return
            
            # Check if download is active
            if download_id in self.active_downloads:
                # Get download item
                download_item = self.active_downloads[download_id]
                
                # Update status
                download_item.status = "cancelled"
                self.signals.status_changed.emit(download_id, "cancelled")
                self.signals.download_finished.emit(download_id, "cancelled", "")
                
                # Add to history
                self.add_to_history(download_item)
                
                # Remove from active downloads
                del self.active_downloads[download_id]
                
                # Thread will be terminated when it checks status
    
    def get_video_info(self, url: str) -> Tuple[List[Dict], List[Dict], bool]:
        """Get video information from URL"""
        ydl_opts = {
            "quiet": True,
            "no_warnings": True,
            "skip_download": True,
            "noplaylist": False,  # Allow playlist detection
        }
        
        try:
            with yt_dlp.YoutubeDL(ydl_opts) as ydl:
                info = ydl.extract_info(url, download=False)
                
                # Check if it's a playlist
                is_playlist = "entries" in info
                
                if is_playlist:
                    # Get first video info for format list
                    first_video = info["entries"][0] if info["entries"] else None
                    if not first_video:
                        return [], [], is_playlist
                    
                    # Get formats
                    formats = self._parse_formats(first_video)
                    
                    # Get subtitle languages
                    subtitles = self._parse_subtitles(first_video)
                    
                    return formats, subtitles, is_playlist
                else:
                    # Get formats
                    formats = self._parse_formats(info)
                    
                    # Get subtitle languages
                    subtitles = self._parse_subtitles(info)
                    
                    return formats, subtitles, is_playlist
        except Exception as e:
            return [], [], False
    
    def _parse_formats(self, info: Dict) -> List[Dict]:
        """Parse video formats"""
        formats = []
        
        # Add audio-only option
        formats.append({
            "format_id": "audio_only",
            "format_note": "Audio only (MP3)",
            "ext": "mp3",
            "resolution": "Audio only",
            "vcodec": "none",
            "acodec": "mp3",
            "filesize": None,
            "audio_only": True
        })
        
        # Add video formats
        if "formats" in info:
            # Filter out audio-only formats and get unique resolutions
            video_formats = []
            seen_resolutions = set()
            
            for f in info["formats"]:
                # Skip audio-only formats
                if f.get("vcodec") == "none":
                    continue
                
                # Get resolution
                width = f.get("width", 0)
                height = f.get("height", 0)
                resolution = f"${width}x{height}" if width and height else "unknown"
                
                # Skip duplicates
                if resolution in seen_resolutions:
                    continue
                
                seen_resolutions.add(resolution)
                
                # Add format
                video_formats.append({
                    "format_id": f["format_id"],
                    "format_note": f.get("format_note", ""),
                    "ext": f.get("ext", "mp4"),
                    "resolution": f"{height}p" if height else "unknown",
                    "vcodec": f.get("vcodec", "unknown"),
                    "acodec": f.get("acodec", "unknown"),
                    "filesize": f.get("filesize"),
                    "audio_only": False
                })
            
            # Sort by resolution (height)
            video_formats.sort(key=lambda x: int(x["resolution"].replace("p", "")) if x["resolution"] != "unknown" else 0, reverse=True)
            
            # Add to formats list
            formats.extend(video_formats)
        
        # Add best format
        formats.insert(1, {
            "format_id": "best",
            "format_note": "Best quality",
            "ext": "mp4",
            "resolution": "Best",
            "vcodec": "best",
            "acodec": "best",
            "filesize": None,
            "audio_only": False
        })
        
        return formats
    
    def _parse_subtitles(self, info: Dict) -> List[Dict]:
        """Parse subtitle languages"""
        subtitles = []
        
        # Add "None" option
        subtitles.append({
            "lang_code": "none",
            "lang_name": "None"
        })
        
        # Add available subtitles
        if "subtitles" in info:
            for lang_code, subtitle_info in info["subtitles"].items():
                # Get language name
                lang_name = subtitle_info[0]["name"] if subtitle_info and "name" in subtitle_info[0] else lang_code
                
                # Add to subtitles list
                subtitles.append({
                    "lang_code": lang_code,
                    "lang_name": lang_name
                })
        
        return subtitles