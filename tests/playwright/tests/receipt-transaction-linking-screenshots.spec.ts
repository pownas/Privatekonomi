import { test, expect } from '@playwright/test';

test.describe('Receipt-Transaction Linking Screenshots', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
  });

  test('01 - Transaction list with receipt indicators', async ({ page }) => {
    // Navigate to transactions page
    await page.goto('/transactions');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Take screenshot showing transactions with receipt indicators
    await page.screenshot({ 
      path: 'screenshots/receipt-transaction-linking/01-transaction-list-with-receipts.png',
      fullPage: false 
    });
    console.log('✓ Transaction list screenshot captured');
  });

  test('02 - Transaction details with receipts section', async ({ page }) => {
    // Navigate to transactions page
    await page.goto('/transactions');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Look for a transaction with receipts (indicated by receipt chip)
    const transactionWithReceipt = page.locator('text=/.*🧾.*/').first();
    
    // If found, click on it
    if (await transactionWithReceipt.count() > 0) {
      await transactionWithReceipt.click();
      await page.waitForTimeout(1000);
      
      // Take screenshot of transaction details dialog
      await page.screenshot({ 
        path: 'screenshots/receipt-transaction-linking/02-transaction-details-receipts-section.png',
        fullPage: false 
      });
      console.log('✓ Transaction details with receipts screenshot captured');
      
      // Close dialog
      await page.keyboard.press('Escape');
    } else {
      console.log('⚠ No transaction with receipts found - skipping');
    }
  });

  test('03 - Receipts table with transaction column', async ({ page }) => {
    // Navigate to receipts page
    await page.goto('/receipts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Take screenshot showing receipts table with transaction column
    await page.screenshot({ 
      path: 'screenshots/receipt-transaction-linking/03-receipts-table-with-transaction-column.png',
      fullPage: false 
    });
    console.log('✓ Receipts table screenshot captured');
  });

  test('04 - Transaction selector dialog', async ({ page }) => {
    // Navigate to receipts page
    await page.goto('/receipts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Look for a link button (receipt not yet linked)
    const linkButton = page.locator('button[title*="Länka till transaktion"]').first();
    
    if (await linkButton.count() > 0) {
      await linkButton.click();
      await page.waitForTimeout(1000);
      
      // Take screenshot of transaction selector dialog
      await page.screenshot({ 
        path: 'screenshots/receipt-transaction-linking/04-transaction-selector-dialog.png',
        fullPage: false 
      });
      console.log('✓ Transaction selector dialog screenshot captured');
      
      // Close dialog
      await page.keyboard.press('Escape');
    } else {
      console.log('⚠ No unlinked receipt found - skipping');
    }
  });

  test('05 - Receipt details with transaction link', async ({ page }) => {
    // Navigate to receipts page
    await page.goto('/receipts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Look for view button
    const viewButton = page.locator('button[title*="Visa"]').first();
    
    if (await viewButton.count() > 0) {
      await viewButton.click();
      await page.waitForTimeout(1000);
      
      // Take screenshot of receipt details
      await page.screenshot({ 
        path: 'screenshots/receipt-transaction-linking/05-receipt-details-with-transaction-link.png',
        fullPage: false 
      });
      console.log('✓ Receipt details screenshot captured');
      
      // Close dialog
      await page.keyboard.press('Escape');
    } else {
      console.log('⚠ No receipts found - skipping');
    }
  });

  test('06 - Unlink confirmation dialog', async ({ page }) => {
    // Navigate to receipts page
    await page.goto('/receipts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Look for an unlink button (receipt is linked)
    const unlinkButton = page.locator('button[title*="Avlänka"]').first();
    
    if (await unlinkButton.count() > 0) {
      await unlinkButton.click();
      await page.waitForTimeout(1000);
      
      // Take screenshot of unlink confirmation
      await page.screenshot({ 
        path: 'screenshots/receipt-transaction-linking/06-unlink-confirmation.png',
        fullPage: false 
      });
      console.log('✓ Unlink confirmation screenshot captured');
      
      // Cancel the dialog
      const cancelButton = page.locator('button:has-text("Avbryt")');
      if (await cancelButton.count() > 0) {
        await cancelButton.click();
      } else {
        await page.keyboard.press('Escape');
      }
    } else {
      console.log('⚠ No linked receipt found - skipping');
    }
  });

  test('07 - Full workflow demonstration', async ({ page }) => {
    // Navigate to receipts page
    await page.goto('/receipts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Take initial state screenshot
    await page.screenshot({ 
      path: 'screenshots/receipt-transaction-linking/07-receipts-page-overview.png',
      fullPage: true 
    });
    console.log('✓ Receipts page overview screenshot captured');
    
    // Navigate to transactions page
    await page.goto('/transactions');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Take transactions overview
    await page.screenshot({ 
      path: 'screenshots/receipt-transaction-linking/08-transactions-page-overview.png',
      fullPage: true 
    });
    console.log('✓ Transactions page overview screenshot captured');
  });

  test('09 - Dark mode versions', async ({ page }) => {
    // Switch to dark mode
    const darkModeButton = page.locator('[aria-label*="Mörkt"]');
    if (await darkModeButton.count() > 0) {
      await darkModeButton.click();
      await page.waitForTimeout(1000);
      
      // Receipts page in dark mode
      await page.goto('/receipts');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      
      await page.screenshot({ 
        path: 'screenshots/receipt-transaction-linking/09-receipts-page-dark-mode.png',
        fullPage: false 
      });
      console.log('✓ Receipts page dark mode screenshot captured');
      
      // Transactions page in dark mode
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      
      await page.screenshot({ 
        path: 'screenshots/receipt-transaction-linking/10-transactions-page-dark-mode.png',
        fullPage: false 
      });
      console.log('✓ Transactions page dark mode screenshot captured');
    }
  });
});
