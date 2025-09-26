# ExcelViewer UI Design System

## Color Palette

### Brand Colors
- **Primary (Navy Blue)**: `#1E3A5F` - Professional, trustworthy
- **Accent (Warm Orange)**: `#FF6B35` - Energy, attention to CTAs
- **Secondary Blue**: `#4A90E2` - Links, secondary actions

### Light Theme

#### Backgrounds
- **Main Background**: `#FFFFFF`
- **Secondary Background**: `#F8F9FA`
- **Tertiary Background**: `#F1F3F5`
- **Card/Panel Background**: `#FFFFFF` with subtle shadow

#### Text Colors
- **Primary Text**: `#212529`
- **Secondary Text**: `#6C757D`
- **Disabled Text**: `#ADB5BD`
- **Link Text**: `#4A90E2`

#### Neutral Grays
- **Gray 100**: `#F8F9FA` - Lightest backgrounds
- **Gray 200**: `#E9ECEF` - Borders, dividers
- **Gray 300**: `#DEE2E6` - Disabled backgrounds
- **Gray 400**: `#CED4DA` - Subtle borders
- **Gray 500**: `#ADB5BD` - Icons, secondary elements
- **Gray 600**: `#6C757D` - Secondary text
- **Gray 700**: `#495057` - Body text
- **Gray 800**: `#343A40` - Headings
- **Gray 900**: `#212529` - Primary text

#### State Colors
- **Success**: `#28A745` / Light: `#D4EDDA`
- **Warning**: `#FFC107` / Light: `#FFF3CD`
- **Error**: `#DC3545` / Light: `#F8D7DA`
- **Info**: `#17A2B8` / Light: `#D1ECF1`

#### Interactive States
- **Hover Background**: `#F1F3F5`
- **Selected Background**: `#E3F2FD` (light blue tint)
- **Active/Pressed**: `#1E3A5F` with 10% opacity
- **Focus Border**: `#FF6B35` (orange accent)

### Dark Theme

#### Backgrounds
- **Main Background**: `#0D1117`
- **Secondary Background**: `#161B22`
- **Tertiary Background**: `#21262D`
- **Card/Panel Background**: `#1C2128` with subtle border

#### Text Colors
- **Primary Text**: `#F0F6FC`
- **Secondary Text**: `#8B949E`
- **Disabled Text**: `#484F58`
- **Link Text**: `#58A6FF`

#### Dark Mode Grays
- **Gray 900**: `#0D1117` - Darkest background
- **Gray 800**: `#161B22` - Secondary background
- **Gray 700**: `#21262D` - Elevated surfaces
- **Gray 600**: `#30363D` - Borders
- **Gray 500**: `#484F58` - Disabled elements
- **Gray 400**: `#6E7681` - Subtle text
- **Gray 300**: `#8B949E` - Secondary text
- **Gray 200**: `#C9D1D9` - Body text
- **Gray 100**: `#F0F6FC` - Primary text

#### State Colors (Dark Mode)
- **Success**: `#3FB950` / Dark: `#0F2E1C`
- **Warning**: `#D29922` / Dark: `#2E2111`
- **Error**: `#F85149` / Dark: `#3D1A1A`
- **Info**: `#58A6FF` / Dark: `#0D2640`

#### Interactive States (Dark Mode)
- **Hover Background**: `#21262D`
- **Selected Background**: `#1E3A5F` with 20% opacity
- **Active/Pressed**: `#FF6B35` with 15% opacity
- **Focus Border**: `#FF6B35`

## Typography

### Font Stack
```css
font-family: 'Segoe UI', 'San Francisco', 'Ubuntu', -apple-system, BlinkMacSystemFont, sans-serif;
font-family-mono: 'Cascadia Code', 'Consolas', 'Monaco', monospace;
```

### Font Sizes
- **Heading 1**: 24px / 32px line-height
- **Heading 2**: 20px / 28px line-height
- **Heading 3**: 16px / 24px line-height
- **Body**: 14px / 20px line-height
- **Small**: 12px / 16px line-height
- **Tiny**: 11px / 14px line-height

## Spacing System

Using 4px base unit:
- **xs**: 4px
- **sm**: 8px
- **md**: 12px
- **lg**: 16px
- **xl**: 24px
- **xxl**: 32px
- **xxxl**: 48px

## Component Specific Styles

### Search Results (Notepad++ Style)

#### Tree Structure Colors
- **Search Query Header**: Navy Blue `#1E3A5F` background, white text
- **File Group**: Light blue tint `#E3F2FD` (light) / `#1A2332` (dark)
- **Sheet Group**: Subtle gray `#F8F9FA` (light) / `#1C2128` (dark)
- **Result Item**: White (light) / `#0D1117` (dark)
- **Selected Result**: Orange highlight `#FF6B35` with 20% opacity
- **Match Highlight**: Yellow background `#FFF3CD`

#### Icons
- **File Icon**: 📄 or custom Excel icon - Navy Blue
- **Sheet Icon**: 📊 or table icon - Gray 600
- **Cell Icon**: ⬚ or cell icon - Gray 500
- **Expanded**: ▼ - Gray 700
- **Collapsed**: ▶ - Gray 700

### Data Tables

#### Headers
- Background: Navy Blue `#1E3A5F` (primary) or Gray 100 (secondary)
- Text: White (on navy) or Gray 900 (on gray)
- Border: Gray 300

#### Cells
- Background: White (light) / Gray 900 (dark)
- Alternate Row: Gray 50 (light) / Gray 800 (dark)
- Border: Gray 200
- Hover: Orange tint with 10% opacity

#### Differences Highlighting
- **Added/New**: Green background `#D4EDDA`
- **Removed**: Red background `#F8D7DA`
- **Modified**: Orange background `#FFE5D3`
- **Match**: No highlight

### Buttons

#### Primary Button (CTA)
- Background: Orange `#FF6B35`
- Text: White
- Hover: Darker Orange `#E55A2B`
- Active: Even Darker `#CC4E22`

#### Secondary Button
- Background: Navy Blue `#1E3A5F`
- Text: White
- Hover: Lighter Navy `#2A4A75`
- Active: Original Navy

#### Tertiary Button
- Background: Transparent
- Text: Navy Blue (light) / Orange (dark)
- Border: 1px solid current color
- Hover: 10% opacity background fill

## Shadow System

```css
/* Elevation levels */
--shadow-sm: 0 1px 2px rgba(0,0,0,0.07);
--shadow-md: 0 2px 4px rgba(0,0,0,0.1);
--shadow-lg: 0 4px 8px rgba(0,0,0,0.12);
--shadow-xl: 0 8px 16px rgba(0,0,0,0.15);
```

## Animation Guidelines

- **Duration**: 150-300ms for micro-interactions
- **Easing**: `ease-out` for enter, `ease-in` for exit
- **Properties**: Transform and opacity preferred over layout properties

## Accessibility

- Minimum contrast ratio: 4.5:1 for normal text, 3:1 for large text
- Focus indicators: 2px solid orange border
- Keyboard navigation: All interactive elements accessible
- Screen reader: Proper ARIA labels and descriptions