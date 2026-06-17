# Simple Assumptions Guide

Here are the basic assumptions we made about the database, passwords, and AI logic to keep this project working:

---

## 1. Database Assumptions
- We assume that every customer gets a unique `CustomerId` number, and every feedback gets a unique `FeedbackId` number that SQL Server generates automatically.
- We assume that the customer database table contains columns for `FullName`, `Email`, `Username`, and `PasswordHash`.
- We assume that the review database table contains columns for `FeedbackText`, `Summary`, `Sentiment`, `IssueCategory`, `RecommendedAction`, and `SubmittedAt` (date of submission).

---

## 2. Passwords and Hashing
- We assume that saving passwords in plain text is unsafe. We use a cryptography standard called PBKDF2 to hash passwords before storing them.
- If the database already has old plain text passwords, the server will check if the passwords match, log the user in, and then automatically convert them to the safe hashed format.

---

## 3. AI Behavior
- When the server sends text to the AI model, we assume the AI will return a valid JSON format.
- We configured a **fallback option**: if the AI API is down or times out, the server will automatically default the sentiment to `"NEUTRAL"` and set the summary to `"Review manually"`. This ensures the review is still saved in the database even if the AI is offline.
