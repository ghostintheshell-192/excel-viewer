# UI Implementation Plan for Sonnet

## Overview
Transform ExcelViewer's UI to be more professional and visually appealing using the design system defined in UI_DESIGN_SYSTEM.md. Support both Light and Dark themes.

## Phase 1: Setup Theme Infrastructure

### 1.1 Create Theme Resources Structure
**File**: `src/ExcelViewer.UI.Avalonia/Styles/Themes/ThemeResources.axaml`

Create a ResourceDictionary with:
- Color definitions for both light/dark themes
- Use DynamicResource for theme switching
- Define all colors from the design system

### 1.2 Create Theme Manager
**File**: `src/ExcelViewer.UI.Avalonia/Services/ThemeManager.cs`

Implement:
- `IThemeManager` interface with `CurrentTheme` property and `ToggleTheme()` method
- Store theme preference in user settings
- Apply theme on application startup
- Notify UI of theme changes

### 1.3 Update App.axaml
- Include theme resources
- Set default theme
- Add global styles

## Phase 2: Search Results UI (Notepad++ Style)

### 2.1 Update TreeSearchResultsView.axaml
**File**: `src/ExcelViewer.UI.Avalonia/Views/TreeSearchResultsView.axaml`

Transform the TreeView to match Notepad++ "Find All" results:

```xaml
<!-- Structure Example -->
<TreeView>
  <!-- Search Query Item Template -->
  <DataTemplate>
    <Border Background="{DynamicResource NavyBlue}" Padding="8,4">
      <TextBlock Foreground="White" FontWeight="SemiBold">
        <Run Text="Search: "/>
        <Run Text="{Binding Query}" FontWeight="Bold"/>
        <Run Text=" ("/>
        <Run Text="{Binding TotalResults}"/>
        <Run Text=" hits in "/>
        <Run Text="{Binding FileCount}"/>
        <Run Text=" files)"/>
      </TextBlock>
    </Border>
  </DataTemplate>

  <!-- File Group Template -->
  <DataTemplate>
    <StackPanel Orientation="Horizontal" Spacing="4">
      <PathIcon Data="{StaticResource ExcelFileIcon}" Fill="{DynamicResource NavyBlue}"/>
      <TextBlock Text="{Binding FileName}" FontWeight="Medium"/>
      <TextBlock Text="(" Foreground="{DynamicResource SecondaryText}"/>
      <TextBlock Text="{Binding ResultCount}" Foreground="{DynamicResource SecondaryText}"/>
      <TextBlock Text=" hits)" Foreground="{DynamicResource SecondaryText}"/>
    </StackPanel>
  </DataTemplate>

  <!-- Result Item Template -->
  <DataTemplate>
    <Border Padding="24,2,8,2" BorderThickness="0,0,0,1"
            BorderBrush="{DynamicResource BorderColor}">
      <Grid ColumnDefinitions="Auto,Auto,*">
        <TextBlock Grid.Column="0" Text="Line " Foreground="{DynamicResource SecondaryText}"/>
        <TextBlock Grid.Column="1" Text="{Binding Row}" Foreground="{DynamicResource AccentOrange}"/>
        <TextBlock Grid.Column="2" Margin="8,0,0,0">
          <!-- Highlight matching text -->
          <Run Text="{Binding PreMatch}"/>
          <Run Text="{Binding Match}" Background="{DynamicResource HighlightYellow}"/>
          <Run Text="{Binding PostMatch}"/>
        </TextBlock>
      </Grid>
    </Border>
  </DataTemplate>
</TreeView>
```

Add:
- Expand/collapse animations
- Hover effects
- Selection highlighting
- Context menu for actions
- Checkboxes for row comparison selection

### 2.2 Create Icon Resources
**File**: `src/ExcelViewer.UI.Avalonia/Styles/Icons.axaml`

Define path data for icons:
- Excel file icon
- Sheet/worksheet icon
- Cell icon
- Expand/collapse chevrons
- Search icon
- Compare icon

## Phase 3: Main Window Enhancement

### 3.1 Update Toolbar
**File**: `src/ExcelViewer.UI.Avalonia/Views/MainWindow.axaml`

Modernize toolbar:
- Flat design with hover effects
- Group related buttons
- Add separators between groups
- Use icons with optional text
- Add theme toggle button

### 3.2 Update Status Bar
Add:
- File count indicator
- Search result count
- Theme indicator
- Memory usage (optional)

## Phase 4: Data Tables Enhancement

### 4.1 Update DataGrid Styles
**File**: `src/ExcelViewer.UI.Avalonia/Styles/DataGridStyles.axaml`

Create custom DataGrid style:
- Modern header style with Navy Blue background
- Alternating row colors
- Hover highlighting
- Cell borders in subtle gray
- Difference highlighting for comparisons

### 4.2 Update RowComparisonView
**File**: `src/ExcelViewer.UI.Avalonia/Views/RowComparisonView.axaml`

Enhance comparison view:
- Color-coded differences (green/red/orange)
- Synchronized scrolling
- Column alignment indicators
- Export button styling

## Phase 5: Dialogs and Popups

### 5.1 Create Modern Dialog Style
**File**: `src/ExcelViewer.UI.Avalonia/Styles/DialogStyles.axaml`

- Rounded corners
- Drop shadow
- Blur background
- Smooth animations

### 5.2 Update All MessageBoxes
Replace default dialogs with styled versions

## Phase 6: Animations and Transitions

### 6.1 Add Micro-interactions
- Button press effects
- Hover transitions
- Tab switching animations
- Loading indicators

### 6.2 Add Page Transitions
- Fade in/out for view changes
- Slide animations for panels

## Phase 7: Final Polish

### 7.1 Responsive Layout
- Ensure proper scaling
- Test different window sizes
- Add minimum window size

### 7.2 Accessibility
- Ensure keyboard navigation works
- Add proper focus indicators
- Test with high contrast mode

### 7.3 Performance
- Test with large datasets
- Optimize rendering
- Add virtualization where needed

## Implementation Order

1. **Start with**: Theme infrastructure (Phase 1)
2. **Then**: Search Results UI (Phase 2) - Most visible improvement
3. **Follow with**: Main Window (Phase 3)
4. **Then**: Data Tables (Phase 4)
5. **Finally**: Polish and animations (Phases 5-7)

## Testing Checklist

- [ ] Light theme looks professional
- [ ] Dark theme has good contrast
- [ ] Theme switching works smoothly
- [ ] Search results are easy to read
- [ ] Hover states work everywhere
- [ ] Focus indicators are visible
- [ ] Colors are consistent
- [ ] Icons are clear and meaningful
- [ ] Animations are smooth
- [ ] Performance is not impacted

## Notes for Sonnet

1. Use `DynamicResource` for all colors to support theme switching
2. Test both themes after each change
3. Keep animations subtle (150-300ms)
4. Ensure all text has sufficient contrast
5. Use the Orange accent sparingly - only for important CTAs and focus
6. Navy Blue is primary for headers and important UI elements
7. Grays for structure and secondary elements
8. Match Notepad++ tree structure for search results familiarity