const apiBaseUrl = process.env.API_BASE_URL ?? 'http://127.0.0.1:5000';

module.exports = [
  {
    context: ['/api'],
    target: apiBaseUrl,
    changeOrigin: true,
    secure: false,
    logLevel: 'warn',
  },
];
