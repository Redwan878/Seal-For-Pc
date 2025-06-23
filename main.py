#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
TreaYT Downloader - A modern YouTube video downloader for Windows
Inspired by Seal for Android
"""

import sys
import os
from PySide6.QtWidgets import QApplication
from PySide6.QtCore import QCoreApplication, Qt
from PySide6.QtGui import QIcon

from src.app import MainWindow
from src.utils.config import Config
from src.utils.dependency_checker import check_dependencies


def setup_application():
    # Set application attributes
    QCoreApplication.setAttribute(Qt.AA_EnableHighDpiScaling, True)
    QCoreApplication.setAttribute(Qt.AA_UseHighDpiPixmaps, True)
    
    # Create application instance
    app = QApplication(sys.argv)
    app.setApplicationName("TreaYT Downloader")
    app.setApplicationVersion("1.0.0")
    app.setOrganizationName("TreaYT")
    app.setOrganizationDomain("treayt.com")
    
    # Set application icon
    if os.path.exists("resources/icon.ico"):
        app.setWindowIcon(QIcon("resources/icon.ico"))
    
    return app


def main():
    # Check for required dependencies
    dependencies_ok = check_dependencies()
    if not dependencies_ok:
        return 1
    
    # Setup application
    app = setup_application()
    
    # Load configuration
    config = Config()
    
    # Create and show main window
    window = MainWindow(config)
    window.show()
    
    # Start application event loop
    return app.exec()


if __name__ == "__main__":
    sys.exit(main())