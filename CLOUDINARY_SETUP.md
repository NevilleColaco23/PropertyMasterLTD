# Cloudinary Setup Instructions

## 🎯 Overview
This project uses Cloudinary to store property logos externally. This prevents database bloat and provides CDN-powered image delivery.

## 📋 Prerequisites
- A Cloudinary account (free tier is sufficient)
- Railway deployment (or any hosting platform)

---

## 🚀 Step-by-Step Setup

### 1. Create Cloudinary Account

1. Go to https://cloudinary.com/users/register/free
2. Sign up for a free account
3. Verify your email

**Free Tier Includes:**
- 25 GB storage
- 25 GB bandwidth/month
- 25,000 transformations/month
- Perfect for small to medium applications!

---

### 2. Get Your Cloud Name

1. Log in to https://console.cloudinary.com/
2. On the Dashboard, you'll see your **Cloud name** (e.g., `dxy1234abc`)
3. Copy this value

---

### 3. Create an Upload Preset (Important!)

An upload preset allows unsigned uploads from the browser (no API secret needed).

1. Go to **Settings** (⚙️ icon in top right)
2. Click **Upload** tab
3. Scroll down to **Upload presets**
4. Click **Add upload preset**

**Configuration:**
- **Signing Mode**: Select **"Unsigned"** ⚠️ (This is crucial!)
- **Preset name**: `property_logos` (or any name you prefer)
- **Folder**: `property-logos` (optional, organizes your images)
- **Resource type**: Image
- **Access mode**: Public
- **Allowed formats**: jpg, png, jpeg, webp
- **File size limit**: 2 MB
- **Transformations** (optional):
  - Width: 800
  - Height: 600
  - Crop: Fill
  - Quality: auto
  - Format: auto (enables automatic WebP conversion)

5. Click **Save**
6. Copy the **Preset name** (e.g., `property_logos`)

---

### 4. Update Environment Configuration

Open `app/src/app/environments/environment.ts` and update:

```typescript
cloudinary: {
  cloudName: 'dxy1234abc',        // ← Your cloud name from step 2
  uploadPreset: 'property_logos',  // ← Your preset name from step 3
  apiKey: '',  // Leave empty for unsigned uploads
}
```

**Example:**
```typescript
cloudinary: {
  cloudName: 'mycompany-staging',
  uploadPreset: 'property_logos_unsigned',
  apiKey: '',
}
```

---

### 5. Test the Upload

1. Run your Angular app: `npm start`
2. Go to Property Master
3. Click "Add Property"
4. Try uploading an image
5. Check the browser console for success/error messages
6. Verify the image appears in your Cloudinary Media Library

---

## 🔍 Verification

### In Cloudinary Dashboard:
1. Go to **Media Library**
2. You should see your uploaded image in the `property-logos` folder
3. Click on the image to see details (URL, transformations, etc.)

### In MongoDB:
1. Check the `Property` collection
2. The `CompanyLogoURL` field should contain a Cloudinary URL like:
   ```
   https://res.cloudinary.com/dxy1234abc/image/upload/v1234567890/property-logos/abc123.jpg
   ```

### In Browser Network Tab:
1. Open DevTools (F12)
2. Go to Network tab
3. Upload an image
4. Look for request to `api.cloudinary.com/v1_1/YOUR_CLOUD_NAME/image/upload`
5. Check response for `secure_url`

---

## 🎨 Optional: Image Transformations

Cloudinary can automatically optimize images on-the-fly using URL parameters.

### Examples:

**Original Image:**
```
https://res.cloudinary.com/demo/image/upload/sample.jpg
```

**Resized to 400x300:**
```
https://res.cloudinary.com/demo/image/upload/w_400,h_300,c_fill/sample.jpg
```

**Auto-format (WebP for modern browsers):**
```
https://res.cloudinary.com/demo/image/upload/f_auto,q_auto/sample.jpg
```

**Rounded corners:**
```
https://res.cloudinary.com/demo/image/upload/r_20/sample.jpg
```

You can add these transformations in your Angular template:
```html
<img [src]="logoUrl | cloudinaryTransform: 'w_400,h_300,c_fill,q_auto,f_auto'">
```

Or create a pipe for this!

---

## 🚨 Troubleshooting

### Error: "Upload preset not found"
- Make sure you created an **unsigned** upload preset
- Double-check the preset name matches exactly (case-sensitive)

### Error: "Unauthorized"
- You're using a signed preset instead of unsigned
- Create a new preset with "Signing Mode: Unsigned"

### Error: "Invalid cloud name"
- Verify your cloud name is correct (no spaces, special chars)
- Check `environment.ts` for typos

### Images not uploading:
1. Check browser console for errors
2. Verify network request to Cloudinary API
3. Check file size (must be < 2MB)
4. Ensure file type is an image

### Images uploading but not saving to database:
- Check that `companyLogoURL` is being sent in the API payload
- Verify backend is saving the URL field correctly

---

## 📊 Monitoring Usage

1. Go to Cloudinary Dashboard
2. Click **Reports** → **Usage**
3. Monitor:
   - Storage used
   - Bandwidth used
   - Transformations used
   - Credits remaining

**Free tier limits:**
- Storage: 25 GB
- Bandwidth: 25 GB/month
- Transformations: 25,000/month

---

## 🔐 Security Best Practices

### ✅ DO:
- Use **unsigned** upload presets for client-side uploads
- Set file size limits in upload preset
- Restrict allowed file formats
- Use folder organization (`property-logos`, `user-avatars`, etc.)
- Enable automatic moderation for inappropriate content

### ❌ DON'T:
- Don't expose your API Secret in frontend code
- Don't allow unlimited file sizes
- Don't allow all file types
- Don't use signed uploads from browser (requires secret key)

---

## 🌍 Production Setup (Railway)

When deploying to Railway, the same configuration works because:

1. **No server-side storage needed** - Images go directly to Cloudinary
2. **No Railway volume needed** - Saves money!
3. **CDN included** - Fast image delivery worldwide
4. **No environment variables needed** - Configuration is in `environment.prod.ts`

Just make sure to update `environment.prod.ts` with the same credentials:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api.railway.app/api/v1',
  cloudinary: {
    cloudName: 'dxy1234abc',
    uploadPreset: 'property_logos',
    apiKey: '',
  }
};
```

---

## 💡 Alternative: Backend Upload (More Secure)

If you want more control, you can upload through your backend:

### Benefits:
- Hide Cloudinary credentials
- Validate images server-side
- Add watermarks
- Resize before upload
- Better security

### Implementation:
1. Install Cloudinary .NET SDK in your API project
2. Create an upload endpoint
3. Upload from Angular to your API
4. Your API uploads to Cloudinary
5. Return URL to Angular

Let me know if you want this implementation!

---

## 📚 Resources

- [Cloudinary Documentation](https://cloudinary.com/documentation)
- [Upload Presets Guide](https://cloudinary.com/documentation/upload_presets)
- [Image Transformations](https://cloudinary.com/documentation/image_transformations)
- [Angular Integration](https://cloudinary.com/documentation/angular_integration)
- [Free Tier Limits](https://cloudinary.com/pricing)

---

## 🎉 You're All Set!

Your property logos are now stored on Cloudinary's global CDN, with automatic optimization and transformation capabilities. Enjoy the benefits of professional image management without the infrastructure headaches!

**Questions?** Check the troubleshooting section or contact support.
