//
//  Environment.swift
//  PersonalAnalytics
//
//  Created by Roy Rutishauser on 25.11.19.
//

import Foundation

public enum Environment {
    // MARK: - Plist
    private static let infoDictionary: [String: Any] = {
        guard let dict = Bundle.main.infoDictionary else {
            fatalError("Plist file not found")
        }
        return dict
    }()

    
    // MARK: - Environment Keys

    static let statusBarIcon: String = {
        guard let icon = Environment.infoDictionary["STATUS_BAR_ICON"] as? String else {
            fatalError("status bar icon not set in plist for this environment")
        }
        return icon
    }()
    
    static let appSupportDir: String = {
        guard let dir = Environment.infoDictionary["APP_SUPPORT_DIR"] as? String else {
            fatalError("app support directory not set in plist for this environment")
        }
        return dir
    }()
    
    static let sqliteDbName: String = {
        guard let dir = Environment.infoDictionary["SQLITE_DB_NAME"] as? String else {
            fatalError("sqlite database name not set in plist for this environment")
        }
        return dir
    }()
    
    static let retrospectivePort: String = {
        guard let port = Environment.infoDictionary["RETROSPECTIVE_PORT"] as? String else {
            fatalError("server port for retrospective not set in plist for this environment")
        }
        return port
    }()
    
    static let errorsLogFile: String = {
        guard let port = Environment.infoDictionary["ERRORS_LOG_FILE"] as? String else {
            fatalError("errors log filename not set in plist for this environment")
        }
        return port
    }()
    
    static let env: String = {
           guard let port = Environment.infoDictionary["ENV"] as? String else {
               fatalError("env shortcut not set in plist for this environment")
           }
           return port
       }()
}
