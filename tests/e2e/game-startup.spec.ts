import { expect, test } from '@playwright/test';

test('player can start a new mission and see the generated recovery code', async ({ page }, testInfo) => {
  await page.goto('/');

  await page.getByLabel('Player Name').fill('Elgin');
  await page.getByLabel('Ship Name').fill('USS Horizon');
  await page.getByRole('button', { name: 'Start New Game' }).click();

  await expect(page.getByRole('heading', { name: 'Hold the Line' })).toBeVisible();
  await expect(page.getByRole('heading', { level: 3 })).toHaveText(/^[A-Z][a-z]+[A-Z][a-z]+\d{3}$/);

  await page.screenshot({ path: testInfo.outputPath('new-game-launch.png'), fullPage: true });

  const resumeCode = await page.getByRole('heading', { level: 3 }).textContent();

  await page.reload();

  await expect(page.getByRole('heading', { name: 'Hold the Line' })).toBeVisible();
  if (resumeCode) {
    await expect(page.getByText(resumeCode)).toBeVisible();
  }
});

test('player can resume an existing session without matching code case', async ({ page }, testInfo) => {
  const created = await page.request.post('/api/sessions', {
    data: {
      playerName: 'Elgin',
      shipName: 'USS Horizon',
    },
  });

  expect(created.ok()).toBeTruthy();
  const session = await created.json();

  await page.goto('/');
  await page.getByLabel('Resume Code').fill(session.resumeCode.toLowerCase());
  await page.getByRole('button', { name: 'Resume Saved Game' }).click();

  await expect(page.getByRole('heading', { name: 'Hold the Line' })).toBeVisible();
  await expect(page.getByText(session.resumeCode)).toBeVisible();
  await expect(page.getByText('Elgin')).toBeVisible();
  await expect(page.getByText('USS Horizon')).toBeVisible();

  await page.screenshot({ path: testInfo.outputPath('resume-session-launch.png'), fullPage: true });

  await page.reload();

  await expect(page.getByRole('heading', { name: 'Hold the Line' })).toBeVisible();
  await expect(page.getByText(session.resumeCode)).toBeVisible();
});
