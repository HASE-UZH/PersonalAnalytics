//
//  MouseActionController.swift
//  PersonalAnalytics
//
//  Created by Chris Satterfield on 2017-05-03.
//

import Foundation

class MouseActionController{

    fileprivate var globalEventMonitor: AnyObject?
    fileprivate var leftClickCountThisInterval: Int
    fileprivate var rightClickCountThisInterval: Int
    fileprivate var scrollDeltaThisInterval: Int
    fileprivate var mouseMovement: Int
    fileprivate var lastMouseLocation: NSPoint

    init(){
        self.leftClickCountThisInterval = 0
        self.rightClickCountThisInterval = 0
        self.scrollDeltaThisInterval = 0
        self.mouseMovement = 0
        self.lastMouseLocation = NSEvent.mouseLocation //add initial mouse location
        
        self.globalEventMonitor = NSEvent.addGlobalMonitorForEvents(matching: [NSEvent.EventTypeMask.leftMouseDown, NSEvent.EventTypeMask.rightMouseDown, NSEvent.EventTypeMask.mouseMoved, NSEvent.EventTypeMask.scrollWheel], handler: self.recordActions) as AnyObject?
    }
    
    func recordActions(mouseEvent:NSEvent){
        //TODO: fill in switch
        switch mouseEvent.type{
        case .leftMouseDown:
            leftClickCountThisInterval += 1
        case .rightMouseDown:
            rightClickCountThisInterval += 1
        case .mouseMoved:
            let currentLocation = NSEvent.mouseLocation
            let distance = calculateDistance(a: currentLocation, b: lastMouseLocation)
            lastMouseLocation = currentLocation
            mouseMovement += distance
        case .scrollWheel:
            let scroll = Int(abs(mouseEvent.scrollingDeltaY))
            scrollDeltaThisInterval += scroll
        default:
            print("Whoops! how did we get here")
        }
    }
    
    func calculateDistance(a: NSPoint, b: NSPoint) -> Int{

        return Int(sqrt(pow(b.x-a.x,2) + pow(b.y-a.y,2)))
    
    }
    
    func reset(){
        self.leftClickCountThisInterval = 0
        self.rightClickCountThisInterval = 0
        self.scrollDeltaThisInterval = 0
        self.mouseMovement = 0
    }
    
    func getValues() -> (Int, Int, Int, Int){
        return (leftClickCountThisInterval, rightClickCountThisInterval, scrollDeltaThisInterval, mouseMovement)
    }
    
    
    deinit{
        if let monitor = self.globalEventMonitor {
            NSEvent.removeMonitor(monitor)
        }
    }
    
}
