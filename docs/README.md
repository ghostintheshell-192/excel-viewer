# ExcelViewer - Documentation Structure

This directory contains all documentation for the ExcelViewer project, organized by purpose.

## 📁 Structure

### `website/`
**GitHub Pages website** - Public-facing project website hosted at https://ghostintheshell-192.github.io/excel-viewer/

**Contents:**
- `index.html` - Main landing page
- `styles/` - CSS stylesheets
- `images/` - Screenshots and visual assets
- `assets/` - Icons, fonts, and other static resources
- `scripts/` - JavaScript for website interactivity

**Deployment:** Automatically published via GitHub Actions (`.github/workflows/deploy-pages.yml`) on every push to `main` branch that modifies files in this directory.

---

### `project/`
**Project documentation** - Technical and business documentation about ExcelViewer

**Contents:**
- `overview.md` - Project overview, goals, and current status
- `technical-specs.md` - Technical architecture and implementation details
- `README.md` - Quick reference for developers

**Audience:** Developers, contributors, technical stakeholders

---

### `development/`
**Development documentation** - Internal planning and design documents

**Contents:**
- `planning/` - Roadmaps and feature planning documents
- `design-reviews/` - API design reviews and technical decisions

**Audience:** Core development team

**Note:** This is committable documentation. For private notes, use `.personal/notes/` (see root `.personal/README.md`).

---

## 🚫 Ignored Directories

The following directories exist locally but are excluded from git (see `.gitignore`):

- `issues/` - Temporary issue tracking (deprecated - use GitHub Issues or `.personal/notes/todo.md`)
- `reports/` - Internal reports and analysis
- `vista.pdf` - Legacy documentation

---

## 🔄 Workflow

### Updating the website
1. Edit files in `docs/website/`
2. Commit and push to `main` branch
3. GitHub Actions automatically deploys changes
4. Website live at https://ghostintheshell-192.github.io/excel-viewer/

### Updating documentation
1. Edit files in `docs/project/` or `docs/development/`
2. Commit and push normally
3. Documentation visible on GitHub repository

---

## 📝 Migration Notes

**Previous structure:** All files were in `docs/` root, mixing website and documentation.

**New structure (Oct 2025):**
- Separated website (`docs/website/`) from documentation
- Enabled flexible GitHub Actions deployment
- Organized documentation by audience and purpose

---

*Last updated: October 2025*
