// AppTrackingTransparency.framework'u import etmeden ObjC runtime ile cagiriyoruz.
// Bu sayede link hatasi olmaz; framework iOS 14+'ta sistem tarafindan yukleniyor.
#import <Foundation/Foundation.h>

typedef void (^ATTCompletionHandler)(NSUInteger);

extern "C" {

void ATT_RequestAuthorization(void) {
    if (@available(iOS 14, *)) {
        Class cls = NSClassFromString(@"ATTrackingManager");
        if (!cls) return;

        SEL sel = NSSelectorFromString(@"requestTrackingAuthorizationWithCompletionHandler:");
        if (![cls respondsToSelector:sel]) return;

        ATTCompletionHandler handler = ^(NSUInteger status) {
            NSLog(@"[ATT] Authorization status: %lu", (unsigned long)status);
        };

        NSMethodSignature *sig = [cls methodSignatureForSelector:sel];
        if (!sig) return;
        NSInvocation *inv = [NSInvocation invocationWithMethodSignature:sig];
        [inv setSelector:sel];
        [inv setTarget:cls];
        [inv setArgument:&handler atIndex:2];
        [inv invoke];
    }
}

int ATT_GetAuthorizationStatus(void) {
    if (@available(iOS 14, *)) {
        Class cls = NSClassFromString(@"ATTrackingManager");
        if (!cls) return 3;

        SEL sel = NSSelectorFromString(@"trackingAuthorizationStatus");
        if (![cls respondsToSelector:sel]) return 3;

        NSMethodSignature *sig = [cls methodSignatureForSelector:sel];
        if (!sig) return 3;
        NSInvocation *inv = [NSInvocation invocationWithMethodSignature:sig];
        [inv setSelector:sel];
        [inv setTarget:cls];
        [inv invoke];

        NSUInteger status = 3;
        [inv getReturnValue:&status];
        return (int)status;
    }
    return 3; // iOS 14 oncesi: Authorized say
}

} // extern "C"
