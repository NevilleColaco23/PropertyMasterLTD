# Vercel Deployment Troubleshooting Guide

## 🚀 Deployment Configuration

### Files Created:
✅ `vercel.json` - Deployment configuration with correct build settings

---

## 📋 Common Deployment Issues & Solutions

### 1. **Module Resolution Errors** (Most Common)

**Error:**
```
Cannot find module '@angular/core'
TS2792: Cannot find module...
```

**Solution:** TypeScript configuration issue. Update `app/tsconfig.json`:

```json
{
  "compilerOptions": {
    "moduleResolution": "node",
    "skipLibCheck": true,
    "resolveJsonModule": true,
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true
  }
}
```

---

### 2. **Build Output Directory Not Found**

**Error:**
```
Error: No Output Directory named "dist" found
```

**Solution:** Already fixed in `vercel.json`:
- Output directory: `app/dist/propertyMasterV3.0/browser`
- Matches Angular project name in `angular.json`

---

### 3. **Out of Memory Errors**

**Error:**
```
FATAL ERROR: Reached heap limit Allocation failed - JavaScript heap out of memory
```

**Solution:** Already configured in `vercel.json`:
```json
"build": {
  "env": {
    "NODE_OPTIONS": "--max_old_space_size=4096"
  }
}
```

---

### 4. **Missing Environment Files**

**Error:**
```
Cannot find file 'src/app/environments/environment.prod.ts'
```

**Solution:** Create environment files or remove from `angular.json`:

**Option A - Create File:**
```typescript
// app/src/app/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-api.com/api'
};
```

**Option B - Remove from angular.json:**
Remove this from `angular.json`:
```json
"fileReplacements": [
  {
    "replace": "src/app/environments/environment.ts",
    "with": "src/app/environments/environment.prod.ts"
  }
]
```

---

### 5. **Budget Size Errors**

**Error:**
```
Error: bundle initial exceeded maximum budget
```

**Solution:** Increase budgets in `angular.json`:

```json
"budgets": [
  {
    "type": "initial",
    "maximumWarning": "2MB",
    "maximumError": "5MB"
  },
  {
    "type": "anyComponentStyle",
    "maximumWarning": "10kB",
    "maximumError": "20kB"
  }
]
```

---

## 🔍 Debug Deployment Issues

### View Complete Error Log:
1. Go to Vercel Dashboard → Your Project → Deployments
2. Click the failed deployment
3. Scroll to **Build Logs** section
4. Copy the **complete error message**

### Common Log Patterns:

**Pattern 1: TypeScript Errors**
```
Error: src/app/component.ts:10:5 - error TS2307: Cannot find module
```
→ Module resolution issue

**Pattern 2: Build Command Failed**
```
Error: Command "npm run build" exited with 1
```
→ Check `package.json` scripts

**Pattern 3: Missing Files**
```
Error: ENOENT: no such file or directory
```
→ File path issue or missing import

---

## ✅ Verify Configuration

### Check `vercel.json`:
```json
{
  "buildCommand": "cd app && npm install && npm run build -- --configuration production",
  "outputDirectory": "app/dist/propertyMasterV3.0/browser"
}
```

### Check `app/package.json`:
```json
{
  "scripts": {
    "build": "ng build",
    "build:prod": "ng build --configuration production"
  }
}
```

### Check `app/angular.json`:
- Project name: `propertyMasterV3.0`
- Output path: Auto-generated as `dist/propertyMasterV3.0/browser`

---

## 🛠️ Manual Build Test (Local)

Test the exact build command Vercel will run:

```bash
cd app
npm install
npm run build -- --configuration production
```

Check if build succeeds locally and output directory is created:
```bash
ls -la app/dist/propertyMasterV3.0/browser
```

You should see:
```
index.html
main.[hash].js
polyfills.[hash].js
styles.[hash].css
assets/
```

---

## 🔄 Redeploy Steps

After fixing configuration:

1. **Commit changes:**
   ```bash
   git add vercel.json
   git commit -m "fix: Add Vercel deployment configuration"
   git push origin 21_Mar_morning
   ```

2. **Vercel Auto-Deploys** from Git push

3. **Or Manual Deploy:**
   - Vercel Dashboard → Your Project → **Redeploy**
   - Check "Use existing build cache" = **NO** (force fresh build)

---

## 📊 Deployment Checklist

Before deploying, verify:

- [ ] `vercel.json` exists in root directory
- [ ] `app/package.json` has correct build script
- [ ] `app/angular.json` has correct project name
- [ ] Environment files exist (if used)
- [ ] Build succeeds locally: `npm run build -- --configuration production`
- [ ] No TypeScript errors: `npm run build -- --configuration production`
- [ ] Output directory created: `app/dist/propertyMasterV3.0/browser/`
- [ ] `index.html` exists in output directory

---

## 🆘 Still Failing?

**Share the complete error log:**

Go to Vercel → Deployments → Failed Build → Copy the **FULL log** starting from:
```
Running "vercel build"
```

Common missing info in error reports:
- What line number is the error on?
- What file is causing the error?
- Is it a build error or runtime error?
- What's the exact error message?

---

## 🎯 Next Steps

1. **Check your latest deployment logs** in Vercel Dashboard
2. **Copy the complete error message** (not just the first line)
3. **Share the error here** so I can provide a specific fix

The `vercel.json` file has been created and configured. Try redeploying now!
