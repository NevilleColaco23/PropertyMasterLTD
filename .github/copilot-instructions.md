# Copilot Instructions

## Project Guidelines
- When implementing social/feed UI features, strictly follow LinkedIn's visual style (card-based posts, distinct spacing, comment sections, stat rows) rather than a flat continuous tile layout.
- In this Angular app, ensure that all feature routes are nested under 'propertyLanding' children in app.routes.ts to preserve the top-level PropertyLandingComponent nav shell and avoid unnecessary menu re-fetching/flicker.