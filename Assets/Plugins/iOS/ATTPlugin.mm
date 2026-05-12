#import <AppTrackingTransparency/AppTrackingTransparency.h>

extern "C" {
    void ATT_RequestAuthorization() {
        if (@available(iOS 14, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                NSLog(@"[ATT] Authorization status: %lu", (unsigned long)status);
            }];
        }
    }

    int ATT_GetAuthorizationStatus() {
        if (@available(iOS 14, *)) {
            return (int)[ATTrackingManager trackingAuthorizationStatus];
        }
        return 3; // iOS 14 oncesi: Authorized say
    }
}
