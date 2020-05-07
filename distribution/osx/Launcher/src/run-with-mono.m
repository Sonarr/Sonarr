#import "run-with-mono.h"

@import Foundation;
@import AppKit;

NSString * const VERSION_TITLE = @"Cannot launch %@";
NSString * const VERSION_MSG = @"%@ requires the Mono Framework version %d.%d or later.";
NSString * const DOWNLOAD_URL = @"http://www.mono-project.com/download/stable/#download-mac";

// Helper method to see if the user has requested debug output
bool D() {
	NSString* v = [[[NSProcessInfo processInfo]environment]objectForKey:@"DEBUG"];
	if (v == nil || v.length == 0 || [v isEqual:@"0"] || [v isEqual:@"false"] || [v isEqual:@"f"])
		return false;
	return true;
}

// Wrapper method to invoke commandline operations and return the string output
NSString *runCommand(NSString *program, NSArray<NSString *> *arguments) {
	NSPipe *pipe = [NSPipe pipe];
	NSFileHandle *file = pipe.fileHandleForReading;

	NSTask *task = [[NSTask alloc] init];
	task.launchPath = program;
	task.arguments = arguments;
	task.standardOutput = pipe;

	[task launch];

	NSData *data = [file readDataToEndOfFile];
	[file closeFile];
	[task waitUntilExit];

	NSString *cmdOutput = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
	if (cmdOutput == nil || cmdOutput.length == 0)
		return nil;

	return [cmdOutput stringByTrimmingCharactersInSet:
                              [NSCharacterSet whitespaceAndNewlineCharacterSet]];
}

// Checks if the Mono version is greater than or equal to the desired version
bool isValidMono(NSString *mono, int major, int minor) {
	NSFileManager *fileManager = [NSFileManager defaultManager];

	if (mono == nil)
		return false;

	if (![fileManager fileExistsAtPath:mono] || ![fileManager isExecutableFileAtPath:mono])
		return false;

	NSString *versionInfo = runCommand(mono, @[@"--version"]);

	NSRange rg = [versionInfo rangeOfString:@"Mono JIT compiler version \\d+\\.\\d+" options:NSRegularExpressionSearch];
	if (rg.location != NSNotFound) {
		versionInfo = [versionInfo substringWithRange:rg];
		if (D()) NSLog(@"Matched version: %@", versionInfo);
		rg = [versionInfo rangeOfString:@"\\d+\\.\\d+" options:NSRegularExpressionSearch];
		if (rg.location != NSNotFound) {
			versionInfo = [versionInfo substringWithRange:rg];
			if (D()) NSLog(@"Matched version: %@", versionInfo);

			NSArray<NSString *> *versionComponents = [versionInfo componentsSeparatedByString:@"."];
			if ([versionComponents[0] intValue] < major)
				return false;
			if ([versionComponents[0] intValue] == major && [versionComponents[1] intValue] < minor)
				return false;

			return true;
		}
	}

	return false;
}

// Attempts to locate a mono with a valid version
NSString *findMono(int major, int minor) {
	NSFileManager *fileManager = [NSFileManager defaultManager];

	NSString *currentMono = runCommand(@"/usr/bin/which", @[@"mono"]);
	if (D()) NSLog(@"which mono: %@", currentMono);

	if (isValidMono(currentMono, major, minor)) {
		if (D()) NSLog(@"Found mono with: %@", currentMono);
		return currentMono;
	}

	NSArray *probepaths = @[@"/usr/local/bin/mono", @"/Library/Frameworks/Mono.framework/Versions/Current/bin/mono", @"/opt/local/bin/mono"];
	for(NSString* probepath in probepaths) {
		if (D()) NSLog(@"Trying mono with: %@", probepath);
		if (isValidMono(probepath, major, minor)) {
			if (D()) NSLog(@"Found mono with: %@", probepath);
			return probepath;
		}
	}

	if (D()) NSLog(@"Failed to find Mono, returning: %@", nil);
	return nil;
}

// Check Bundle for quarantine
void checkBundle() {
    		
	NSString * const bundlePath = [[NSBundle mainBundle] bundlePath];
	NSString * const attributes = runCommand(@"/usr/bin/xattr", @[@"-l", bundlePath]);
	if (D()) NSLog(@"Attributes: %@", attributes);
	if ([attributes containsString:@"com.apple.quarantine:"]) {
		runCommand(@"/usr/bin/xattr", @[@"-dr", @"com.apple.quarantine", bundlePath]);
		NSLog(@"Removed quarantine attribute from bundle");
	}
}


@implementation RunWithMono

+ (void) openDownloadLink:(NSButton*)button {
	if (D()) NSLog(@"Clicked Download");
	runCommand(@"/usr/bin/open", @[DOWNLOAD_URL]);
}

// Shows the download dialog, prompting to download Mono
+ (bool) showDownloadMonoDialog:(NSString *)appName major:(int)major minor:(int)minor {
	NSAlert *alert = [[NSAlert alloc] init];

	[alert setInformativeText:[NSString stringWithFormat:VERSION_MSG, appName, major, minor]];
	[alert setMessageText:[NSString stringWithFormat:VERSION_TITLE, appName]];
	[alert addButtonWithTitle:@"Cancel"];
	[alert addButtonWithTitle:@"Retry"];
	[alert addButtonWithTitle:@"Download"];

	NSButton *downloadButton = [[alert buttons] objectAtIndex:2];

	[downloadButton setTarget:self];
	[downloadButton setAction:@selector(openDownloadLink:)];

	NSModalResponse btn = [alert runModal];
	if (btn == NSAlertFirstButtonReturn) {
		if (D()) NSLog(@"Clicked Cancel");
		return true;
	}
	else if (btn == NSAlertSecondButtonReturn) {
		if (D()) NSLog(@"Clicked Retry");
		return false;
	}

	return true;
}

// Top-level method, finds Mono with an appropriate version and launches the assembly
+ (int) runAssemblyWithMono: (NSString *)appName procnamesuffix:(NSString *)procnamesuffix assembly:(NSString *)assembly major:(int) major minor:(int) minor {
	NSFileManager *fileManager = [NSFileManager defaultManager];

	NSString *assemblyPath;
	bool found = false;

	NSString *localPath = NSProcessInfo.processInfo.arguments[0].stringByDeletingLastPathComponent;
	NSString *resourcePath = [[NSBundle mainBundle] resourcePath];
	NSArray *paths = @[
		localPath,
		[NSString pathWithComponents:@[localPath, @"bin"]],
		resourcePath,
		[NSString pathWithComponents:@[resourcePath, @"bin"]]
	];
	for (NSString* entryFolder in paths) {
		if (D()) NSLog(@"Checking folder: %@", entryFolder);

		assemblyPath = [NSString pathWithComponents:@[entryFolder, assembly]];

		if ([fileManager fileExistsAtPath:assemblyPath]) {
			found = true;
			break;
		}
	}

	if (!found) {
		NSLog(@"Assembly file not found");
		return 1;
	}

	if (D()) NSLog(@"assemblyPath: %@", assemblyPath);

	checkBundle();

	NSString *currentMono = findMono(major, minor);
	
	while (currentMono == nil) {
		NSLog(@"No valid mono found!");
		bool close = [self showDownloadMonoDialog:appName major:major minor:minor];
		if (close)
			return 1;
		currentMono = findMono(major, minor);
	}

	// Setup dylib fallback loading
	NSMutableArray * dylibPath = [NSMutableArray arrayWithObject:assemblyPath.stringByDeletingLastPathComponent];

	// Update the PATH to use the specified mono version
	if ([currentMono hasPrefix:@"/"])
	{
		NSString * curMonoBinDir = currentMono.stringByDeletingLastPathComponent;
		NSString * curMonoDir = curMonoBinDir.stringByDeletingLastPathComponent;
		NSString * curMonoLibDir = [NSString pathWithComponents:@[curMonoDir, @"lib"]];
		
		NSString * curEnvPath = [NSString stringWithUTF8String:getenv("PATH")];
		NSString * newEnvPath = [NSString stringWithFormat:@"%@:%@", curMonoBinDir, curEnvPath];
		setenv("PATH", newEnvPath.UTF8String, 1);

		[dylibPath addObject:curMonoLibDir];

		NSLog(@"Added %@ to PATH", curMonoBinDir);
	}

	// Setup libsqlite?
	/*	if [[ -f '/opt/local/lib/libsqlite3.0.dylib' ]]; then
			export DYLD_FALLBACK_LIBRARY_PATH="/opt/local/lib:$DYLD_FALLBACK_LIBRARY_PATH"
		fi
	*/
	
	[dylibPath addObjectsFromArray:@[@"$HOME/lib", @"/usr/local/lib", @"/lib", @"/usr/lib"]];

	setenv("DYLD_FALLBACK_LIBRARY_PATH", [dylibPath componentsJoinedByString:@":"].UTF8String, 1);

	if (D()) NSLog(@"Running %@ --debug %@", currentMono, assemblyPath);
	
	// Copy commandline arguments
	NSMutableArray* arguments = [[NSMutableArray alloc] init];
	// Disabled suffix for now coz it's confusing and not preserved on in-app restart
	[arguments addObject:currentMono];
	//[arguments addObject:[currentMono stringByAppendingString:procnamesuffix]];
	[arguments addObject:@"--debug"];
	[arguments addObjectsFromArray:[[NSProcessInfo processInfo] arguments]];
	
	// replace the executable-path with the assembly path
	[arguments replaceObjectAtIndex:2 withObject:assemblyPath];

	// Try switch to mono using execv
	char * cPath = strdup([currentMono UTF8String]);
	char ** cArgs;
	char ** pArgNext = cArgs = malloc(sizeof(*cArgs) * ([arguments count] + 1));
	for (NSString *s in arguments) {
		*pArgNext++ = strdup([s UTF8String]);
	}
	*pArgNext = NULL;
	int ret = execv(cPath, cArgs);
	if (ret != 0)
		NSLog(@"Failed execv with errno @d", errno);
	// execv failed, cleanup
	pArgNext = cArgs;
	for (NSString *s in arguments) {
		free(*pArgNext++);
	}
	free(cArgs);
	free(cPath);

	return -1;
}

@end