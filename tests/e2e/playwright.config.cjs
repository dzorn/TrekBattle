module.exports = {
  testDir: '.',
  timeout: 30000,
  expect: {
    timeout: 5000,
  },
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? 'http://127.0.0.1:4300',
    browserName: 'chromium',
    channel: 'chrome',
    headless: true,
    ignoreHTTPSErrors: true,
    viewport: { width: 1440, height: 1080 },
    trace: 'on-first-retry',
  },
};
