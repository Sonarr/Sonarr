@import Foundation;
@import AppKit;

@interface RunWithMono : NSObject {
}

+ (void) openDownloadLink:(NSButton*)button;
+ (bool) showDownloadMonoDialog:(NSString *)appName major:(int)major minor:(int)minor;
+ (int) runAssemblyWithMono:(NSString *)appName procnamesuffix:(NSString *)procnamesuffix assembly:(NSString *)assembly major:(int) major minor:(int) minor;

@end