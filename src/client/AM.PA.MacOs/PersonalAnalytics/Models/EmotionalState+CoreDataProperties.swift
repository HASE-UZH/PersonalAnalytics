//
//  EmotionalState+CoreDataProperties.swift
//  
//
//  Created by Luigi Quaranta on 02/01/2019.
//
//

import Foundation
import CoreData


extension EmotionalState {

    @nonobjc public class func fetchRequest() -> NSFetchRequest<EmotionalState> {
        return NSFetchRequest<EmotionalState>(entityName: "EmotionalState")
    }

    @NSManaged public var timestamp: NSDate?
    @NSManaged public var activity: String?
    @NSManaged public var valence: NSNumber?
    @NSManaged public var arousal: NSNumber?

}
