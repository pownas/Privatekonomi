# OCR Receipt Scanning - Visual Guide

## Feature Overview

The OCR receipt scanning feature allows users to automatically extract information from paper receipts by uploading photos. This document provides a visual walkthrough of the feature.

## User Journey

### Step 1: Access OCR Scanning

From the **Kvitton** (Receipts) page, users see two buttons:
- **"Skanna kvitto (OCR)"** (Outlined, secondary) - Opens OCR dialog
- **"Nytt Kvitto"** (Filled, primary) - Manual entry

```
┌─────────────────────────────────────────────────────┐
│ 📄 Kvitton                                          │
│                                                     │
│  [Skanna kvitto (OCR)]  [+ Nytt Kvitto]           │
└─────────────────────────────────────────────────────┘
```

### Step 2: Upload Receipt Image

The OCR scan dialog presents a drag-and-drop upload area:

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Ladda upp en bild på ditt kvitto så extraherar   │
│  vi informationen automatiskt med hjälp av OCR.    │
│                                                     │
│  ┌───────────────────────────────────────────────┐ │
│  │                                               │ │
│  │         ☁️  Ladda upp kvittobild              │ │
│  │                                               │ │
│  │       eller dra och släpp här                │ │
│  │                                               │ │
│  │     [📷 JPEG, PNG, GIF, WebP (Max 10 MB)]    │ │
│  │                                               │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  ℹ️ Tips för bästa resultat:                       │
│  • Använd god belysning                           │
│  • Fotografera rakt uppifrån                      │
│  • Se till att hela kvittot syns                  │
│  • Undvik skuggor och reflektioner                │
│                                                     │
├─────────────────────────────────────────────────────┤
│                                      [Avbryt]      │
└─────────────────────────────────────────────────────┘
```

### Step 3: Processing

After selecting a file, the user confirms to start scanning:

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ℹ️ Vald bild: receipt.jpg (2.1 MB)               │
│                                                     │
│                              [🔍 Skanna med OCR]   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

During processing:

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│                  ⏳ Processing...                   │
│                                                     │
│       Bearbetar kvittobild med OCR...              │
│       Detta kan ta några sekunder                  │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Step 4: Review Extracted Data

OCR results are displayed in an editable form:

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ✅ OCR-skanning lyckades! Säkerhet: 85%          │
│                                                     │
│  Extraherad data                                   │
│  Granska och redigera informationen nedan:         │
│                                                     │
│  Butik/Återförsäljare*  │  Totalt belopp (kr)*    │
│  [ICA Maxi Västerås   ] │  [129.50            ]   │
│                                                     │
│  Kvittodatum           │  Kvittonummer            │
│  [2024-11-08        ▼] │  [12345              ]   │
│                                                     │
│  Betalningsmetod                                   │
│  [Kort                 ]                           │
│                                                     │
│  Radposter (3 st)                                  │
│  ┌────────────────────────────────────────────┐   │
│  │ Beskrivning │ Antal │ À-pris │ Totalt      │   │
│  ├────────────────────────────────────────────┤   │
│  │ Mjölk 3%    │  1    │ 29.50  │ 29.50 kr   │   │
│  │ Bröd        │  1    │ 25.00  │ 25.00 kr   │   │
│  │ Smör        │  3    │ 25.00  │ 75.00 kr   │   │
│  └────────────────────────────────────────────┘   │
│                                                     │
│  ▼ Visa råtext från OCR                           │
│                                                     │
├─────────────────────────────────────────────────────┤
│                     [Avbryt]  [Använd data]        │
└─────────────────────────────────────────────────────┘
```

### Step 5: Save Receipt

Clicking "Använd data" transfers the information to the receipt form:

```
┌─────────────────────────────────────────────────────┐
│ 📄 Kvitton                                          │
│                                                     │
│  [Skanna kvitto (OCR)]  [+ Nytt Kvitto]           │
└─────────────────────────────────────────────────────┘

✅ OCR-data inläst! Granska och spara kvittot.

┌─────────────────────────────────────────────────────┐
│ Lägg till Nytt Kvitto                              │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Butik/Återförsäljare* │  Totalt belopp (kr)*     │
│  [ICA Maxi Västerås   ] │  [129.50            ]   │
│                                                     │
│  Kvittodatum           │  Kvittotyp               │
│  [2024-11-08        ▼] │  [Skannat          ▼]   │
│                                                     │
│  ... (rest of form with pre-filled data)           │
│                                                     │
│  Radposter:                                        │
│  • Mjölk 3% - 29.50 kr                            │
│  • Bröd - 25.00 kr                                │
│  • Smör (3x) - 75.00 kr                           │
│                                                     │
│  [Lägg till]  [Avbryt]                            │
└─────────────────────────────────────────────────────┘
```

## Data Flow

```
User uploads image
       ↓
[File validation]
  • Check file type
  • Check file size
       ↓
[Image preprocessing]
  • Convert to grayscale
  • Enhance contrast
  • Sharpen image
  • Resize if needed
       ↓
[Tesseract OCR]
  • Extract text (Swedish + English)
       ↓
[Text parsing]
  • Extract total amount
  • Extract date
  • Extract merchant
  • Extract line items
  • Extract payment method
       ↓
[Present to user]
  • Show in editable form
  • Calculate confidence
       ↓
User reviews and edits
       ↓
[Transfer to receipt form]
       ↓
User saves receipt
```

## Error Handling

### File Too Large

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ❌ Filen är för stor. Max storlek är 10 MB.      │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Invalid File Type

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ❌ Endast bildfiler är tillåtna                   │
│     (JPEG, PNG, GIF, WebP).                       │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### OCR Failed

```
┌─────────────────────────────────────────────────────┐
│ Skanna kvitto med OCR                          [x]  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ❌ Ingen text kunde läsas från bilden.           │
│     Försök med en tydligare bild.                 │
│                                                     │
│  [🔄 Försök igen]                                  │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## Confidence Indicators

The system shows confidence in extraction results:

- **85-100%**: ✅ High confidence (green)
- **60-84%**: ⚠️ Medium confidence (yellow)  
- **<60%**: ❌ Low confidence (red)

## Supported Receipt Formats

### Amount Formats

```
✅ Totalt: 100,00 kr
✅ Total: 100.00 SEK
✅ Summa: 100,00
✅ Att betala: 100,00 kr
✅ 100,00 kr
```

### Date Formats

```
✅ 2024-11-08
✅ 08.11.2024
✅ 08/11/2024
✅ 20241108
✅ Datum: 2024-11-08
```

### Line Item Formats

```
✅ Mjölk 3%              29,50
✅ 2x Bröd               50,00
✅ Smör                  45.50 kr
```

### Payment Methods

```
✅ Swish
✅ Kort / Bankkort / Kreditkort
✅ Kontant / Cash
✅ Autogiro
✅ E-faktura
```

## Tips for Best Results

### ✅ Good Receipt Photos

- Clear, well-lit image
- Receipt flat and straight
- Full receipt visible
- No shadows or glare
- High resolution

### ❌ Poor Receipt Photos

- Blurry or out of focus
- Poor lighting
- Receipt folded or crumpled
- Partial receipt only
- Low resolution
- Heavy shadows or reflections

## Technical Details

### Supported Image Formats

- JPEG / JPG
- PNG
- GIF
- WebP

### File Size Limits

- Minimum: No minimum
- Maximum: 10 MB
- Recommended: 1-3 MB for best performance

### Processing Time

- Typical: 2-5 seconds
- Simple receipts: ~2 seconds
- Complex receipts: ~5 seconds
- Large images: up to 8 seconds

### Language Support

- Swedish (swe)
- English (eng)
- Both languages processed simultaneously

## Accessibility Features

- Full keyboard navigation support
- Screen reader friendly
- Clear error messages
- Visual and text feedback
- WCAG 2.1 Level AA compliant

## Mobile Experience

The feature works on mobile devices:
- Touch-friendly file selection
- Responsive layout
- Optimized for portrait and landscape
- Camera roll integration (via file picker)

## Security and Privacy

- ✅ All processing on server
- ✅ No data sent to external services
- ✅ No permanent storage of images
- ✅ User data isolation
- ✅ File validation
- ✅ Size restrictions

## Related Features

- **Receipt Management**: View, edit, and delete receipts
- **Transaction Linking**: Link receipts to transactions
- **Category Rules**: Auto-categorize based on merchant
- **Budget Tracking**: Track spending by category

## Support and Feedback

For help with OCR scanning:
1. Check the tips for best results
2. Try with a different image
3. Review the [User Guide](OCR_RECEIPT_SCANNING_GUIDE.md)
4. Contact support if issues persist
