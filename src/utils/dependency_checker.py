#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Utility to check for required dependencies
"""

import os
import sys
import subprocess
import shutil
from PySide6.QtWidgets import QMessageBox


def check_dependencies():
    """Check if all required dependencies are installed"""
    # Check for yt-dlp
    yt_dlp_installed = check_yt_dlp()
    
    # Check for FFmpeg
    ffmpeg_installed = check_ffmpeg()
    
    # If any dependency is missing, show error message
    if not yt_dlp_installed or not ffmpeg_installed:
        error_message = "The following dependencies are missing:\n"
        
        if not yt_dlp_installed:
            error_message += "- yt-dlp\n"
        
        if not ffmpeg_installed:
            error_message += "- FFmpeg\n"
        
        error_message += "\nPlease install the missing dependencies and try again."
        
        # Show error message
        msg_box = QMessageBox()
        msg_box.setIcon(QMessageBox.Critical)
        msg_box.setWindowTitle("Missing Dependencies")
        msg_box.setText(error_message)
        msg_box.setInformativeText("Would you like to install the missing dependencies automatically?")
        msg_box.setStandardButtons(QMessageBox.Yes | QMessageBox.No)
        msg_box.setDefaultButton(QMessageBox.Yes)
        
        # If user wants to install dependencies automatically
        if msg_box.exec() == QMessageBox.Yes:
            install_dependencies(not yt_dlp_installed, not ffmpeg_installed)
            
            # Check again after installation
            yt_dlp_installed = check_yt_dlp()
            ffmpeg_installed = check_ffmpeg()
            
            if not yt_dlp_installed or not ffmpeg_installed:
                QMessageBox.critical(
                    None,
                    "Installation Failed",
                    "Failed to install dependencies. Please install them manually."
                )
                return False
            
            return True
        
        return False
    
    return True


def check_yt_dlp():
    """Check if yt-dlp is installed"""
    try:
        # Try to import yt_dlp module
        import yt_dlp
        return True
    except ImportError:
        # Check if yt-dlp executable is in PATH
        return shutil.which("yt-dlp") is not None


def check_ffmpeg():
    """Check if FFmpeg is installed"""
    return shutil.which("ffmpeg") is not None


def install_dependencies(install_yt_dlp, install_ffmpeg):
    """Install missing dependencies"""
    if install_yt_dlp:
        try:
            subprocess.check_call([sys.executable, "-m", "pip", "install", "yt-dlp"])
        except subprocess.CalledProcessError:
            pass
    
    if install_ffmpeg:
        # FFmpeg installation is more complex and varies by platform
        # For Windows, we can download a pre-built binary
        if os.name == "nt":  # Windows
            try:
                # Use pip to install ffmpeg-python which includes binaries
                subprocess.check_call([sys.executable, "-m", "pip", "install", "ffmpeg-python"])
            except subprocess.CalledProcessError:
                pass
        else:
            # For other platforms, show instructions
            QMessageBox.information(
                None,
                "FFmpeg Installation",
                "Please install FFmpeg manually for your platform."
            )