export const environment = {
  production: true,
  apiUrl: 'https://propertymaster-api-de09-h0hqbzgsc4arfvez.francecentral-01.azurewebsites.net/api/v1',

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