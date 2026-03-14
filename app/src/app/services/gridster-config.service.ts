import { GridsterConfig, GridsterItem } from 'angular-gridster2';

export class GridsterConfigService {
  /**
   * Get default gridster configuration
   */
  static getDefaultConfig(editMode: boolean = false): GridsterConfig {
    return {
      gridType: 'fixed',  // Use 'fixed' to respect fixedRowHeight
      compactType: 'none',
      margin: 10,
      outerMargin: true,
      outerMarginTop: null,
      outerMarginRight: null,
      outerMarginBottom: null,
      outerMarginLeft: null,
      useTransformPositioning: true,
      mobileBreakpoint: 640,
      minCols: 12,
      maxCols: 12,
      minRows: 1,
      maxRows: 100,
      maxItemCols: 12,
      minItemCols: 1,
      maxItemRows: 100,
      minItemRows: 1,
      maxItemArea: 2500,
      minItemArea: 1,
      defaultItemCols: 3,
      defaultItemRows: 2,
      fixedColWidth: 105,
      fixedRowHeight: 150,  // Increased from 105 to 150 for better visibility
      keepFixedHeightInMobile: false,
      keepFixedWidthInMobile: false,
      scrollSensitivity: 10,
      scrollSpeed: 20,
      enableEmptyCellClick: false,
      enableEmptyCellContextMenu: false,
      enableEmptyCellDrop: false,
      enableEmptyCellDrag: false,
      enableOccupiedCellDrop: false,
      emptyCellDragMaxCols: 50,
      emptyCellDragMaxRows: 50,
      ignoreMarginInRow: false,
      draggable: {
        enabled: editMode,
        ignoreContentClass: 'gridster-item-content',
        ignoreContent: false,
        dragHandleClass: 'drag-handler',
        stop: undefined
      },
      resizable: {
        enabled: editMode,
        handles: {
          s: true,
          e: true,
          n: true,
          w: true,
          se: true,
          ne: true,
          sw: true,
          nw: true
        },
        stop: undefined
      },
      swap: false,
      pushItems: true,
      disablePushOnDrag: false,
      disablePushOnResize: false,
      pushDirections: { north: true, east: true, south: true, west: true },
      pushResizeItems: false,
      displayGrid: editMode ? 'always' : 'none',
      disableWindowResize: false,
      disableWarnings: false,
      scrollToNewItems: false
    };
  }

  /**
   * Convert widget configuration to gridster item
   */
  static toGridsterItem(widgetConfig: any): GridsterItem {
    return {
      x: widgetConfig.position?.x || 0,
      y: widgetConfig.position?.y || 0,
      cols: widgetConfig.position?.width || 3,
      rows: widgetConfig.position?.height || 2,
      ...widgetConfig
    };
  }

  /**
   * Convert gridster item to widget configuration
   */
  static toWidgetConfig(gridsterItem: any): any {
    return {
      widgetId: gridsterItem.widgetId,
      widgetType: gridsterItem.widgetType,
      position: {
        x: gridsterItem.x!,
        y: gridsterItem.y!,
        width: gridsterItem.cols!,
        height: gridsterItem.rows!
      },
      settings: gridsterItem.settings || {}
    };
  }
}
