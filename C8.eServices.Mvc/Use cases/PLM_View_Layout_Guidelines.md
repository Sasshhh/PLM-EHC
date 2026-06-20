# PLM View Layout Guidelines

This document outlines the standard structural pattern that must be used when creating or modifying views in the PLM (Property Lease Management) application.

## Core Principles

1. **Consistent Panel Structure**: All pages must use a standard white panel background (`panel-group` > `panel panel-default`). Do NOT place content raw on the page body.
2. **Standard Headings**: Page titles and section headers must be placed inside a `panel-heading` with `align="center"`.
3. **Content Wrapper**: The actual content (tables, forms, etc.) must reside inside a `panel-collapse collapse in tablecss` div.
4. **Tables**: Use standard Bootstrap table classes: `table table-bordered table-hover table-striped`.
5. **Buttons**: Use standard PLM buttons like `btn btn-primary` rather than custom gradient buttons.

## Standard Layout Template

Whenever you create a new index/dashboard or list view, wrap the content in the following HTML structure:

```html
<div class="panel-group">
    <div class="panel panel-default">
        
        <!-- Page Heading Section -->
        <div class="panel-group" style="margin-bottom: -1px;">
            <div class="panel panel-default">
                <div class="panel-heading" align="center">
                    Page Title Here
                </div>
            </div>
        </div>

        <!-- Main Content Area -->
        <div id="mainContentCollapse" class="panel-collapse collapse in tablecss">
            <br />
            
            <!-- Optional: Action buttons (like Create New) -->
            <div style="padding-left: 15px; margin-bottom: 15px;">
                @Html.ActionLink("Create New", "Create", null, new { @class = "btn btn-primary" })
            </div>

            <!-- Table Structure -->
            <table id="MainTable" class="table table-bordered table-hover table-striped panel panel-default">
                <thead>
                    <tr>
                        <th>Column 1</th>
                        <th>Column 2</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    <!-- Table Rows -->
                    <tr>
                        <td>Data 1</td>
                        <td>Data 2</td>
                        <td>
                            @Html.ActionLink("View", "Details", new { id = 1 }, new { @class = "btn btn-primary" })
                        </td>
                    </tr>
                </tbody>
            </table>
            <br />
        </div>
        
    </div>
</div>
```

## Important Notes
- **Strict Container Boundaries**: You must maintain the standard nested panel structure so that the content doesn't bleed out or break the page layout. Do NOT break out of the main white background panel.
- **Internal Styling**: While the outer layout must use the standard panels, you are encouraged to get "fancy" inside the content wrapper. You can use modern styling, blue highlights, cards, badges, and nice typography *inside* the content div, as long as it is well-organized and looks professional.
- **DataTables**: For tables with data, ensure they are initialized with jQuery DataTables in the `@section Scripts` area.
