# 🎨 Dashboard UI Quick Reference

## Color Palette

### Widget Gradients
```css
/* List Widget - Purple */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

/* Chart Widget - Pink */
background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);

/* Calendar Widget - Blue */
background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
```

### Dashboard Colors
```css
/* Container Background */
background: #f8f9fa;

/* Widget Card Background */
background: white;

/* Edit Mode Header */
background: rgba(25, 118, 210, 0.05);

/* Shadows */
widget-shadow: 0 1px 4px rgba(0,0,0,0.08);
hover-shadow: 0 4px 12px rgba(0,0,0,0.12);
header-shadow: 0 1px 3px rgba(0,0,0,0.06);
```

---

## Spacing System

### Grid Spacing
```typescript
margin: 12px           // Between widgets
outerMarginTop: 0      // No top margin (cleaner)
```

### Widget Padding
```css
/* KPI Cards */
mat-card-content: 20px

/* List/Chart/Calendar Headers */
padding: 16px 20px

/* Edit Mode Header */
padding: 8px 12px
```

---

## Typography Scale

### KPI Cards
```css
.kpi-title: 13px / 600 / uppercase / letter-spacing: 0.5px
.kpi-value: 36px / 700 / line-height: 1
.kpi-trend: 14px / normal

.kpi-icon: 28px x 28px
```

### Headers
```css
mat-card-title: 16px / 600
.item-count: 12px / 600
.current-month: 14px / 600
```

---

## Border & Radius

### Border Radius
```css
widget-card: 8px
dashboard-header: 8px
item-count-badge: 12px
```

### Borders
```css
/* KPI Cards */
border-left: 4px solid (normal)
border-left: 6px solid (hover)

/* Edit Mode Header */
border-bottom: 1px solid rgba(25, 118, 210, 0.1)
```

---

## Shadows & Depth

### Widget Cards
```css
/* Normal */
box-shadow: 0 1px 4px rgba(0,0,0,0.08)

/* Hover */
box-shadow: 0 4px 12px rgba(0,0,0,0.12)
transform: translateY(-2px)

/* Dragging/Resizing */
box-shadow: 0 8px 24px rgba(0,0,0,0.2)
```

### Dashboard Header
```css
box-shadow: 0 1px 3px rgba(0,0,0,0.06)
```

---

## Transitions

### Standard Timing
```css
transition: all 0.3s ease
```

### Specific Properties
```css
/* Widget hover */
transition: box-shadow 0.3s ease, transform 0.3s ease

/* KPI border hover */
transition: all 0.2s ease
```

---

## Layout Grid

### Gridster Configuration
```typescript
gridType: 'fixed'
minCols: 12
maxCols: 12
fixedColWidth: 105px
fixedRowHeight: 150px
```

### Default Widget Sizes
```typescript
/* KPI Cards */
cols: 3, rows: 2  // 315px x 310px

/* Charts */
cols: 6, rows: 4  // 630px x 630px

/* Lists */
cols: 3, rows: 4  // 315px x 630px

/* Calendars */
cols: 3, rows: 4  // 315px x 630px
```

---

## Responsive Breakpoints

```typescript
mobileBreakpoint: 640px
```

### Mobile Adjustments
```css
@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
  }
  
  .dashboard-selector {
    width: 100%;
  }
}
```

---

## Icon Sizes

```css
/* Dashboard Header */
h1 mat-icon: 36px x 36px

/* KPI Cards */
kpi-icon: 28px x 28px
trend-icon: 18px x 18px

/* Loading Spinner */
diameter: 40px (widgets)
diameter: default (dashboard)
```

---

## Button Styles

### Edit Mode Buttons
```css
/* Remove Button */
background: #f44336
color: white
hover: #d32f2f

/* Drag Handle */
color: #666
hover: #1976d2
cursor: move
```

---

## State Colors

### Trend Indicators
```css
.trend-up: #4caf50    /* Green */
.trend-down: #f44336   /* Red */
```

### Edit Mode
```css
header-background: rgba(25, 118, 210, 0.05)  /* Light blue */
border-color: rgba(25, 118, 210, 0.1)
```

---

## Component Structure

### Widget Card (Normal Mode)
```
┌─────────────────────────────────┐
│  mat-card.widget-card           │ ← 8px border-radius
│                                 │
│  ┌───────────────────────────┐ │
│  │ mat-card-content          │ │ ← padding: 0
│  │                           │ │
│  │  app-kpi-card-widget     │ │ ← :host { height: 100% }
│  │  ┌─────────────────────┐ │ │
│  │  │ .kpi-card           │ │ │ ← padding: 20px
│  │  │ - icon + title      │ │ │
│  │  │ - value (36px)      │ │ │
│  │  │ - trend             │ │ │
│  │  └─────────────────────┘ │ │
│  └───────────────────────────┘ │
└─────────────────────────────────┘
```

### Widget Card (Edit Mode)
```
┌─────────────────────────────────┐
│  mat-card.widget-card           │
│  ┌───────────────────────────┐ │
│  │ mat-card-header           │ │ ← rgba(25,118,210,0.05)
│  │  [X]            [≡]       │ │ ← Remove + Drag
│  └───────────────────────────┘ │
│  ┌───────────────────────────┐ │
│  │ mat-card-content          │ │
│  │   (widget component)      │ │
│  └───────────────────────────┘ │
└─────────────────────────────────┘
```

---

## CSS Class Reference

### Dashboard Container
```css
.dashboard-container       /* Main wrapper */
.dashboard-header         /* Top controls */
.header-left             /* Title section */
.header-right            /* Buttons section */
.dashboard-selector      /* Dropdown */
.dashboard-subtitle      /* Welcome text */
```

### Widget Classes
```css
.widget-card              /* Main card */
.widget-edit-header       /* Edit mode header */
.widget-remove-btn        /* Delete button */
.widget-drag-handle       /* Drag icon */
.widget-loading          /* Loading state */
```

### State Classes
```css
.no-header               /* Content without header */
.empty-dashboard         /* No widgets state */
.loading-container       /* Dashboard loading */
.unsaved-badge          /* Unsaved indicator */
```

---

## Animation Keyframes

### Pulse (Unsaved Badge)
```css
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

animation: pulse 1.5s infinite;
```

---

## Z-Index Hierarchy

```css
widget-remove-btn: 10     /* Highest */
widget-drag-handle: 5     /* Medium */
widget-card: 1           /* Base */
gridster: 0              /* Background */
```

---

## Vendor Prefix Requirements

### Not Needed (Modern Browsers):
- ✅ `border-radius`
- ✅ `box-shadow`
- ✅ `transform`
- ✅ `transition`
- ✅ `linear-gradient`

All CSS is modern, no prefixes required! 🎉

---

## Browser Support

- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Edge 90+

**No IE11 support** (uses modern CSS Grid, flexbox, gradients)

---

## Performance Notes

### Optimized:
- ✅ CSS-only animations (no JS)
- ✅ `transform` for smooth animations
- ✅ Minimal repaints
- ✅ GPU-accelerated transitions

### Avoid:
- ❌ Animating `width`/`height` (use `transform: scale`)
- ❌ Animating `margin`/`padding` (use `transform`)
- ❌ Box-shadow in loops (use sparingly)

---

**Quick Copy Reference for Developers** 📋
