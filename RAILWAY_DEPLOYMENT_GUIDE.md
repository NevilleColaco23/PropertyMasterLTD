# Railway Deployment Checklist - Fix CORS 502 Error

## Changes Made:
✅ Added Vercel origin to CORS allowed origins
✅ Fixed middleware order (CORS before Authentication)
✅ Disabled HTTPS redirection in production
✅ Configured proper port binding for Railway
✅ Created Dockerfile and railway.json for testAngularAPI.Server

## Environment Variables to Set on Railway:

### Required:
```
CorsSettings__AllowedOrigins__0=http://localhost:4200
CorsSettings__AllowedOrigins__1=https://localhost:4200
CorsSettings__AllowedOrigins__2=https://property-master-silk.vercel.app
```

### MongoDB Connection:
```
ConnectionStrings__MongoDb=mongodb://mongo:EcMPWsskZQhygpalucMlhpEwbxaapZFL@gondola.proxy.rlwy.net:31267
```

### Authentication:
```
AuthenticationSettings__JwtSigningKeyBase64=e09053f6847d466b8f243d9522c340ef234sdfds
```

### Other Settings:
```
ASPNETCORE_ENVIRONMENT=Production
ApplicationDbSettings__AutoMigrate=true
ApplicationDbSettings__AutoSeed=true
```

## Deployment Steps:

### 1. Commit and Push Changes:
```bash
git add .
git commit -m "Fix CORS and Railway deployment configuration"
git push origin vercel_deployment
```

### 2. Configure Railway Project:

#### Option A: Deploy testAngularAPI.Server (Recommended)
1. Go to Railway dashboard → Your project
2. Go to Settings → Build & Deploy
3. Set **Root Directory**: `testAngularAPI.Server`
4. Set **Build Command**: Leave default (uses Dockerfile)
5. Add the environment variables listed above

#### Option B: Continue Using WebApi
If you want to keep using WebApi (currently deployed):
1. The WebApi project is already configured
2. Just add the environment variables above
3. Redeploy

### 3. Redeploy:
- Railway should auto-deploy after push
- Or manually trigger: **Deployments → Deploy**

### 4. Verify Deployment:
After deployment completes, test:
```bash
# Test health endpoint
curl https://theretreatapp.up.railway.app/health

# Test CORS preflight
curl -X OPTIONS https://theretreatapp.up.railway.app/api/v1/account/login \
  -H "Origin: https://property-master-silk.vercel.app" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type" \
  -v
```

Look for these headers in the response:
- `Access-Control-Allow-Origin: https://property-master-silk.vercel.app`
- `Access-Control-Allow-Methods: POST, GET, OPTIONS, etc.`
- `Access-Control-Allow-Credentials: true`

### 5. Check Logs:
If still getting errors, check Railway logs:
- Railway Dashboard → Deployments → View Logs
- Look for startup errors or CORS-related messages

## Common Issues:

### 502 Bad Gateway:
- Application not starting → Check MongoDB connection string
- Port binding issue → Verify entrypoint.sh is executable
- Missing dependencies → Check Dockerfile build logs

### CORS Still Blocked:
- Environment variables not set → Double-check Railway env vars
- Old deployment cached → Force redeploy
- Wrong origin → Verify exact URL matches (no trailing slash)

## Quick Test from Browser Console:
```javascript
fetch('https://theretreatapp.up.railway.app/api/v1/account/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include',
  body: JSON.stringify({ username: 'test', password: 'test' })
})
.then(r => r.json())
.then(console.log)
.catch(console.error);
```

## Files Modified:
- `testAngularAPI.Server/Startup.cs` - Fixed middleware order
- `testAngularAPI.Server/CORS/CorsStartup.cs` - Use config for origins
- `testAngularAPI.Server/appsettings.json` - Added Vercel origin
- `testAngularAPI.Server/Dockerfile` - New
- `testAngularAPI.Server/entrypoint.sh` - New
- `testAngularAPI.Server/railway.json` - New
- `WebApi/Program.cs` - Added port configuration
- `WebApi/appsettings.json` - Added Vercel origin

## Support:
If issues persist, check:
1. Railway build logs
2. Railway deployment logs  
3. Browser Network tab (look for exact error messages)
4. Railway environment variables are correctly set
