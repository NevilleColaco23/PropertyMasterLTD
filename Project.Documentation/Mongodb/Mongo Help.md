# 📚 MongoDB Query Reference Guide

> A comprehensive guide for common MongoDB operations used in development

---

## 📑 Table of Contents
- [➕ Adding Fields](#-adding-fields)
- [🗑️ Deleting Documents](#️-deleting-documents)
- [🔄 Conditional Updates](#-conditional-updates)

---

## ➕ Adding Fields

### 1️⃣ Add a Field to All Documents

**Description:** Add a new field with a constant value to all documents in a collection.

```javascript
db.getCollection("Property").updateMany(
  {},  // Select all documents
  {
    $set: {
      CompanyLogoURL: "http" 
    }
  }
)
```

**📌 Use Case:** When you need to initialize a new field across your entire collection.

---

### 2️⃣ Add a Field with Default Value

**Description:** Set a default value for a new field across all documents.

```javascript
db.yourCollection.updateMany(
  {},  // Match all documents
  { 
    $set: { 
      emailConfirmed: false 
    } 
  }
)
```

**📌 Use Case:** Adding boolean flags or default configurations to existing documents.

---

## 🗑️ Deleting Documents

### 1️⃣ Delete Documents by ID Range

**Description:** Remove documents where the `_id` is greater than a specified value.

```javascript
db.Users.deleteMany({ 
  _id: { $gt: 3 } 
});
```

**📌 Use Case:** Cleaning up test data or removing documents created after a certain point.

**⚠️ WARNING:** This operation is **irreversible**. Always backup your data before performing bulk deletes!

---

## 🔄 Conditional Updates

### 1️⃣ Add Field Only If It Doesn't Exist

**Description:** Prevent overwriting existing field values by only adding the field when it's missing.

```javascript
db.yourCollection.updateMany(
  { 
    emailConfirmed: { $exists: false } 
  },
  { 
    $set: { 
      emailConfirmed: false 
    } 
  }
)
```

**📌 Use Case:** Safe migration of schema changes without affecting documents that already have the field.

*Last Updated: 2024 | Maintained by Development Team*
