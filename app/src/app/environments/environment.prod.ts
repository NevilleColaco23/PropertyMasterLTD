export const environment = {
  production: true,
  // From `terraform output -raw api_url` (+ /api/v1). Update if the app is recreated under another name.
  apiUrl: 'https://propertymaster-api.azurewebsites.net/api/v1',

  // Cloudinary Configuration
  cloudinary: {
    cloudName: 'dsgimrtxr',
    uploadPreset: 'propertymaster', // Settings > Upload > Upload presets (unsigned)
    apiKey: '', // Optional: Only needed for authenticated requests
  },

  // Production - NO test data
  testData: {
    signup: {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
      phone: '',
      propertyCode: ''
    },
    login: {
      email: '',
      password: ''
    }
  }
};