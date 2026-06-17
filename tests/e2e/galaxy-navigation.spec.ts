import { expect, test } from '@playwright/test';

function parseLocation(locationText: string): { x: number; y: number } {
  const [xText, yText] = locationText.split(',');
  const x = Number.parseInt(xText ?? '', 10) - 1;
  const y = Number.parseInt(yText ?? '', 10) - 1;

  if (Number.isNaN(x) || Number.isNaN(y)) {
    throw new Error(`Could not parse location text: ${locationText}`);
  }

  return { x, y };
}

function chooseWarpDestination(currentX: number, currentY: number): { x: number; y: number } {
  if (currentX < 11) {
    return { x: currentX + 1, y: currentY };
  }

  return { x: currentX - 1, y: currentY };
}

test('player can scan, warp, and end a turn in the galaxy view', async ({ page }) => {
  const created = await page.request.post('/api/sessions', {
    data: {
      playerName: 'Elgin',
      shipName: 'USS Horizon',
    },
  });

  expect(created.ok()).toBeTruthy();
  const session = await created.json();

  await page.goto(`/launch/${session.resumeCode}`);
  await page.getByRole('button', { name: 'Enter Galaxy' }).click();

  await expect(page.getByTestId('turn-counter')).toHaveText('0');
  await expect(page.locator('[data-testid^="galaxy-cell-"]')).toHaveCount(144);
  await expect(page.locator('[data-testid^="nav-cell-"]')).toHaveCount(121);

  const currentLocationText = await page.getByTestId('current-location').textContent();
  if (!currentLocationText) {
    throw new Error('Current location was not displayed.');
  }

  const currentLocation = parseLocation(currentLocationText);
  const scanTarget = currentLocation.x < 11 ? { x: currentLocation.x + 1, y: currentLocation.y } : { x: currentLocation.x - 1, y: currentLocation.y };
  const warpDestination = chooseWarpDestination(currentLocation.x, currentLocation.y);

  await expect(page.getByTestId(`galaxy-cell-${scanTarget.x}-${scanTarget.y}`)).toContainText('Fog');
  await page.getByRole('button', { name: 'Long Range Scan' }).click();
  await expect(page.getByTestId(`galaxy-cell-${scanTarget.x}-${scanTarget.y}`)).not.toContainText('Fog');
  await expect(page.getByTestId('turn-counter')).toHaveText('0');

  await page.getByTestId(`nav-cell-${warpDestination.x}-${warpDestination.y}`).click();

  const updatedLocationText = await page.getByTestId('current-location').textContent();
  if (!updatedLocationText) {
    throw new Error('Updated location was not displayed after warp.');
  }

  const updatedLocation = parseLocation(updatedLocationText);
  expect(updatedLocation).toEqual(warpDestination);
  await expect(page.getByTestId('turn-counter')).toHaveText('0');

  await page.getByRole('button', { name: 'End Turn' }).click();
  await expect(page.getByTestId('turn-counter')).toHaveText('1');
});
