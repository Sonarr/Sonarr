#import "run-with-mono.h"
#import "PFMoveApplication.h"

int const MONO_VERSION_MAJOR = 5;
int const MONO_VERSION_MINOR = 20;

int main() {
	@autoreleasepool {
		// Use our own executable name so the same compiled binary to be used for forks
		NSString * const FileName = NSProcessInfo.processInfo.arguments[0].lastPathComponent;

		// Sonarr.Update.exe
		NSString * const ASSEMBLY = [NSString stringWithFormat:@"%@.exe", FileName];

		// Sonarr Update
		NSString * const APP_NAME = [FileName stringByReplacingOccurrencesOfString:@"." withString:@" "];
		
		// -sonarrupdate
		NSString * const PROCESS_NAME = [NSString stringWithFormat:@"-%@", [FileName stringByReplacingOccurrencesOfString:@"." withString:@""].lowercaseString];
		
		@try
		{
			PFMoveToApplicationsFolderIfNecessary();
		}
		@catch (NSException * ex)
		{
			NSLog(@"Translocation/Quarantine check failed, starting normally. Reason: %@", ex.reason);
		}

		return [RunWithMono runAssemblyWithMono:APP_NAME procnamesuffix:PROCESS_NAME assembly:ASSEMBLY major:MONO_VERSION_MAJOR minor:MONO_VERSION_MINOR];
	}
}
