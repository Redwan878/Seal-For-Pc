#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Main application window for TreaYT Downloader
"""

import os
from PySide6.QtWidgets import (
    QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
    QPushButton, QLabel, QLineEdit, QComboBox, 
    QProgressBar, QTabWidget, QFileDialog, QMessageBox,
    QCheckBox, QSpinBox, QListWidget, QListWidgetItem,
    QSplitter, QFrame, QSizePolicy
)
from PySide6.QtCore import Qt, QSize, Signal, Slot, QUrl, QSettings
from PySide6.QtGui import QIcon, QDesktopServices, QColor, QPalette

from .widgets.download_item import DownloadItem
from .widgets.video_info import VideoInfoWidget
from .widgets.history_widget import HistoryWidget
from .widgets.settings_widget import SettingsWidget
from .utils.downloader import Downloader
from .utils.config import Config
from .utils.theme_manager import ThemeManager


class MainWindow(QMainWindow):
    """Main application window"""
    
    def __init__(self, config):
        super().__init__()
        
        self.config = config
        self.theme_manager = ThemeManager(self)
        self.downloader = Downloader(self)
        self.downloads = []
        self.history = []
        
        self.init_ui()
        self.load_settings()
        
        # Apply theme based on settings
        self.apply_theme(self.config.get("dark_mode", False))
    
    def init_ui(self):
        """Initialize the user interface"""
        # Set window properties
        self.setWindowTitle("TreaYT Downloader")
        self.setMinimumSize(900, 600)
        
        # Create central widget and main layout
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout(central_widget)
        
        # Create URL input section
        url_layout = QHBoxLayout()
        url_label = QLabel("YouTube URL:")
        self.url_input = QLineEdit()
        self.url_input.setPlaceholderText("Paste YouTube URL here...")
        self.analyze_button = QPushButton("Analyze")
        self.analyze_button.clicked.connect(self.analyze_url)
        
        url_layout.addWidget(url_label)
        url_layout.addWidget(self.url_input)
        url_layout.addWidget(self.analyze_button)
        
        main_layout.addLayout(url_layout)
        
        # Create tab widget for different sections
        self.tabs = QTabWidget()
        
        # Download tab
        self.download_tab = QWidget()
        download_layout = QVBoxLayout(self.download_tab)
        
        # Video info widget
        self.video_info = VideoInfoWidget()
        download_layout.addWidget(self.video_info)
        
        # Download options
        options_layout = QHBoxLayout()
        
        # Format selection
        format_layout = QVBoxLayout()
        format_label = QLabel("Format:")
        self.format_combo = QComboBox()
        self.format_combo.addItems(["Best Quality", "1080p", "720p", "480p", "360p", "Audio Only (MP3)", "Audio Only (M4A)"])
        format_layout.addWidget(format_label)
        format_layout.addWidget(self.format_combo)
        options_layout.addLayout(format_layout)
        
        # Subtitle options
        subtitle_layout = QVBoxLayout()
        self.subtitle_check = QCheckBox("Download Subtitles")
        self.subtitle_combo = QComboBox()
        self.subtitle_combo.addItems(["English", "Auto-generated", "All Available"])
        self.subtitle_combo.setEnabled(False)
        self.subtitle_check.toggled.connect(self.subtitle_combo.setEnabled)
        subtitle_layout.addWidget(self.subtitle_check)
        subtitle_layout.addWidget(self.subtitle_combo)
        options_layout.addLayout(subtitle_layout)
        
        # Playlist options
        playlist_layout = QVBoxLayout()
        self.playlist_check = QCheckBox("Download Playlist")
        self.playlist_check.setEnabled(False)
        playlist_layout.addWidget(self.playlist_check)
        options_layout.addLayout(playlist_layout)
        
        # Output directory
        output_layout = QVBoxLayout()
        output_label = QLabel("Output Directory:")
        output_dir_layout = QHBoxLayout()
        self.output_dir = QLineEdit()
        self.output_dir.setReadOnly(True)
        self.output_dir.setText(os.path.expanduser("~/Downloads"))
        browse_button = QPushButton("Browse")
        browse_button.clicked.connect(self.browse_output_dir)
        output_dir_layout.addWidget(self.output_dir)
        output_dir_layout.addWidget(browse_button)
        output_layout.addWidget(output_label)
        output_layout.addLayout(output_dir_layout)
        
        download_layout.addLayout(options_layout)
        download_layout.addLayout(output_layout)
        
        # Download button
        self.download_button = QPushButton("Download")
        self.download_button.setEnabled(False)
        self.download_button.clicked.connect(self.start_download)
        download_layout.addWidget(self.download_button)
        
        # Active downloads section
        downloads_label = QLabel("Active Downloads:")
        self.downloads_list = QListWidget()
        self.downloads_list.setMinimumHeight(200)
        download_layout.addWidget(downloads_label)
        download_layout.addWidget(self.downloads_list)
        
        # History tab
        self.history_tab = HistoryWidget()
        
        # Settings tab
        self.settings_tab = SettingsWidget(self.config, self.theme_manager)
        
        # Add tabs to tab widget
        self.tabs.addTab(self.download_tab, "Download")
        self.tabs.addTab(self.history_tab, "History")
        self.tabs.addTab(self.settings_tab, "Settings")
        
        main_layout.addWidget(self.tabs)
        
        # Status bar
        self.statusBar().showMessage("Ready")
    
    def analyze_url(self):
        """Analyze the provided YouTube URL"""
        url = self.url_input.text().strip()
        if not url:
            QMessageBox.warning(self, "Error", "Please enter a YouTube URL")
            return
        
        self.statusBar().showMessage("Analyzing URL...")
        self.analyze_button.setEnabled(False)
        
        # Start analysis in a separate thread
        self.downloader.analyze_url(url, self.analysis_complete)
    
    def analysis_complete(self, result, data):
        """Handle URL analysis completion"""
        self.analyze_button.setEnabled(True)
        
        if not result:
            QMessageBox.warning(self, "Error", f"Failed to analyze URL: {data}")
            self.statusBar().showMessage("Analysis failed")
            return
        
        # Update UI with video information
        self.video_info.update_info(data)
        self.download_button.setEnabled(True)
        
        # Enable playlist checkbox if it's a playlist
        self.playlist_check.setEnabled("playlist" in data and data["playlist"])
        
        self.statusBar().showMessage("Analysis complete")
    
    def browse_output_dir(self):
        """Open file dialog to select output directory"""
        directory = QFileDialog.getExistingDirectory(
            self, "Select Output Directory", 
            self.output_dir.text(),
            QFileDialog.ShowDirsOnly
        )
        
        if directory:
            self.output_dir.setText(directory)
    
    def start_download(self):
        """Start the download process"""
        url = self.url_input.text().strip()
        output_dir = self.output_dir.text()
        
        # Get selected format
        format_index = self.format_combo.currentIndex()
        format_option = self.format_combo.currentText()
        
        # Get subtitle options
        subtitles = False
        subtitle_lang = None
        if self.subtitle_check.isChecked():
            subtitles = True
            subtitle_lang = self.subtitle_combo.currentText()
        
        # Get playlist option
        playlist = self.playlist_check.isChecked() if self.playlist_check.isEnabled() else False
        
        # Create download options dictionary
        options = {
            "url": url,
            "output_dir": output_dir,
            "format": format_option,
            "subtitles": subtitles,
            "subtitle_lang": subtitle_lang,
            "playlist": playlist
        }
        
        # Create download item widget
        download_item = DownloadItem(options, self.video_info.get_info())
        
        # Add to downloads list
        item = QListWidgetItem()
        item.setSizeHint(download_item.sizeHint())
        self.downloads_list.addItem(item)
        self.downloads_list.setItemWidget(item, download_item)
        
        # Add to downloads tracking
        self.downloads.append({
            "item": item,
            "widget": download_item,
            "options": options
        })
        
        # Start download
        self.downloader.download(options, download_item)
        
        # Add to history
        self.history_tab.add_history_item(options, self.video_info.get_info())
        
        # Reset UI
        self.download_button.setEnabled(False)
        self.video_info.clear()
        self.url_input.clear()
        self.statusBar().showMessage("Download started")
    
    def apply_theme(self, dark_mode):
        """Apply light or dark theme"""
        self.theme_manager.apply_theme(dark_mode)
    
    def load_settings(self):
        """Load application settings"""
        settings = QSettings()
        
        # Load window geometry
        geometry = settings.value("geometry")
        if geometry:
            self.restoreGeometry(geometry)
        
        # Load window state
        state = settings.value("windowState")
        if state:
            self.restoreState(state)
        
        # Load output directory
        output_dir = settings.value("output_dir")
        if output_dir and os.path.exists(output_dir):
            self.output_dir.setText(output_dir)
    
    def save_settings(self):
        """Save application settings"""
        settings = QSettings()
        
        # Save window geometry and state
        settings.setValue("geometry", self.saveGeometry())
        settings.setValue("windowState", self.saveState())
        
        # Save output directory
        settings.setValue("output_dir", self.output_dir.text())
    
    def closeEvent(self, event):
        """Handle window close event"""
        # Check if downloads are in progress
        active_downloads = False
        for download in self.downloads:
            if download["widget"].is_active():
                active_downloads = True
                break
        
        if active_downloads:
            reply = QMessageBox.question(
                self, "Confirm Exit",
                "Downloads are still in progress. Are you sure you want to exit?",
                QMessageBox.Yes | QMessageBox.No,
                QMessageBox.No
            )
            
            if reply == QMessageBox.No:
                event.ignore()
                return
        
        # Save settings
        self.save_settings()
        
        # Accept the event
        event.accept()