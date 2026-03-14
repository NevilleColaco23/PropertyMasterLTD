
export const environment = {
  production: true,
  apiUrl: 'https://theretreatapp.up.railway.app/api/v1',

  // Cloudinary Configuration
  // TODO: Replace these with your actual Cloudinary production credentials
  cloudinary: {
    cloudName: 'dsgimrtxr',
    uploadPreset: 'propertymaster',  //Settings > Upload > Upload presets (unsigned)
    apiKey: '',  // Optional: Only needed for authenticated requests
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
