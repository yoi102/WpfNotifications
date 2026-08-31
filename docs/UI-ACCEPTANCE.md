# UI acceptance checklist

Run this checklist before publishing a release that changes notification layout, animation, focus, or overlay behavior.

## Display matrix

- Windows 11 at 100%, 125%, 150%, and 200% display scaling.
- One monitor and at least one mixed-DPI, two-monitor arrangement.
- Primary, mouse-pointer, and owner-window overlay targets.
- Top-left, top-right, bottom-left, bottom-right, and center positions.
- Light, dark, and high-contrast Windows themes.

## Interaction

- Toggle the close button and countdown bar independently in both sample applications.
- Confirm hidden countdown bars do not prevent timed expiration.
- Confirm hover and keyboard focus pause and resume expiration.
- Close an in-window notification with the button and the Escape key.
- Confirm overlay notifications do not activate their window or steal focus.
- Clear in-window and overlay targets while several notifications are active.
- Verify permanent, overflow, duplicate-tag update, and replacement behavior.

## Accessibility and resilience

- Inspect close-button name and notification live-region output with Accessibility Insights or Narrator.
- Enable “Show animations in Windows” off and verify immediate, stable transitions.
- Resize the sample window to its minimum size and check for clipped controls.
- Enter high-contrast mode and verify text, status feedback, and focus indicators remain visible.
- Disconnect or rearrange a secondary monitor between overlay notifications and verify new overlays stay inside a work area.

Record the Windows build, GPU, monitor scaling, theme, and result for each release candidate.
