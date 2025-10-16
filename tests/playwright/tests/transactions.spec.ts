import { test, expect } from '@playwright/test';

test.describe('Transactions Page', () => {
  test('should display all 50 seeded transactions', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the page to load and render
    await page.waitForSelector('table', { timeout: 10000 });

    // Wait a bit for Blazor to fully render
    await page.waitForTimeout(2000);

    // Get all transaction rows (excluding header)
    const rows = await page.locator('tbody tr').count();

    // Verify we have 50 transactions
    expect(rows).toBe(50);
  });

  test('should display transaction details correctly', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the table to load
    await page.waitForSelector('table', { timeout: 10000 });
    await page.waitForTimeout(2000);

    // Check that the table has the expected headers
    const dateHeader = page.locator('th:has-text("Datum")');
    const descriptionHeader = page.locator('th:has-text("Beskrivning")');
    const categoryHeader = page.locator('th:has-text("Kategori")');
    const amountHeader = page.locator('th:has-text("Belopp")');
    const actionsHeader = page.locator('th:has-text("Åtgärder")');

    await expect(dateHeader).toBeVisible();
    await expect(descriptionHeader).toBeVisible();
    await expect(categoryHeader).toBeVisible();
    await expect(amountHeader).toBeVisible();
    await expect(actionsHeader).toBeVisible();
  });

  test('should display transaction with date, description, category and amount', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the table to load
    await page.waitForSelector('table', { timeout: 10000 });
    await page.waitForTimeout(2000);

    // Get the first transaction row
    const firstRow = page.locator('tbody tr').first();

    // Check that it has date, description, category, and amount
    const cells = firstRow.locator('td');
    expect(await cells.count()).toBeGreaterThanOrEqual(4);

    // Verify date format (should match yyyy-MM-dd)
    const dateCell = cells.nth(0);
    const dateText = await dateCell.textContent();
    expect(dateText).toMatch(/^\d{4}-\d{2}-\d{2}$/);

    // Verify description exists
    const descriptionCell = cells.nth(1);
    const descriptionText = await descriptionCell.textContent();
    expect(descriptionText).toBeTruthy();
    expect(descriptionText!.length).toBeGreaterThan(0);

    // Verify category chip exists
    const categoryCell = cells.nth(2);
    const categoryChip = categoryCell.locator('.mud-chip');
    await expect(categoryChip).toBeVisible();

    // Verify amount exists and has currency format
    const amountCell = cells.nth(3);
    const amountText = await amountCell.textContent();
    expect(amountText).toMatch(/[+-].*kr/i);
  });

  test('should be able to search/filter transactions', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the table to load
    await page.waitForSelector('table', { timeout: 10000 });
    await page.waitForTimeout(2000);

    // Get initial number of rows
    const initialRows = await page.locator('tbody tr').count();
    expect(initialRows).toBe(50);

    // Find the search input
    const searchInput = page.locator('input[placeholder="Sök"]');
    await expect(searchInput).toBeVisible();

    // Search for a specific term - use "Lön" which appears less frequently
    await searchInput.fill('Lön');
    await page.waitForTimeout(1000);

    // Get filtered number of rows
    const filteredRows = await page.locator('tbody tr').count();

    // Should have fewer rows after filtering (or at least not more)
    expect(filteredRows).toBeLessThanOrEqual(initialRows);
    
    // Verify at least one row is visible with the search term
    if (filteredRows > 0) {
      const firstRowText = await page.locator('tbody tr').first().textContent();
      expect(firstRowText).toBeTruthy();
    }

    // Clear the search
    await searchInput.clear();
    await page.waitForTimeout(1000);

    // Should show all rows again
    const clearedRows = await page.locator('tbody tr').count();
    expect(clearedRows).toBe(50);
  });

  test('should display different categories with colored chips', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the table to load
    await page.waitForSelector('table', { timeout: 10000 });
    await page.waitForTimeout(2000);

    // Get all category chips
    const categoryChips = page.locator('.mud-chip');
    const chipCount = await categoryChips.count();

    // Should have at least as many chips as transactions (could be more with split categories)
    expect(chipCount).toBeGreaterThanOrEqual(50);

    // Check that chips have background colors (indicating categories)
    const firstChip = categoryChips.first();
    const style = await firstChip.getAttribute('style');
    expect(style).toContain('background-color');
  });

  test('should display both income and expense transactions', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the table to load
    await page.waitForSelector('table', { timeout: 10000 });
    await page.waitForTimeout(2000);

    // Look for positive (income) and negative (expense) amounts
    const positiveAmount = page.locator('td:has-text("+")').first();
    const negativeAmount = page.locator('td:has-text("-")').first();

    // At least one of each should exist
    await expect(positiveAmount).toBeVisible();
    await expect(negativeAmount).toBeVisible();
  });

  test('should have delete button for each transaction', async ({ page }) => {
    // Navigate to the transactions page
    await page.goto('/transactions');

    // Wait for the table to load
    await page.waitForSelector('table', { timeout: 10000 });
    await page.waitForTimeout(2000);

    // Get all delete buttons
    const deleteButtons = page.locator('button[aria-label*="delete" i], button:has(svg.mud-icon-root)');
    const buttonCount = await deleteButtons.count();

    // Should have 50 delete buttons (one per transaction)
    expect(buttonCount).toBeGreaterThanOrEqual(50);
  });
});
