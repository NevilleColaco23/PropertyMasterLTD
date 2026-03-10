
export const environment = {
  production: true,
  apiUrl: 'https://theretreatapp.up.railway.app/api/v1',

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
