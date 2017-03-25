//
//  Plugin.m
//  iCloudKeyValue
//
//  Created by Gennadii Potapov on 30/7/16.
//  Copyright © 2016 General Arcade. All rights reserved.
//

#import <Foundation/Foundation.h>

char* cStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

void defaultSetString(char * key, NSString * value) {
    [[[NSUserDefaults alloc] initWithSuiteName:@"group.nvdb"] setObject:value forKey:[NSString stringWithUTF8String:key]];
    [[[NSUserDefaults alloc] initWithSuiteName:@"group.nvdb"] synchronize];
}

const char* defaultGetString(char * key) {
    NSUserDefaults* defaults = [[NSUserDefaults alloc] initWithSuiteName:@"group.nvdb"];
    NSString* str = [defaults stringForKey:[NSString stringWithUTF8String:key]];
    return cStringCopy([str UTF8String]);
}
