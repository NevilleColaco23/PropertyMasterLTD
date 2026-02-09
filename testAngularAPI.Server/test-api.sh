#!/bin/bash

# Test script for User Authentication API
# This script demonstrates how to get the active user ID in the API

API_URL="http://localhost:5128"

echo "=========================================="
echo "Testing User Authentication API"
echo "=========================================="
echo ""

# Step 1: Create a test user
echo "Step 1: Creating a test user..."
CREATE_RESPONSE=$(curl -s -X POST "$API_URL/user" \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}' \
  --max-time 10)

echo "Response: $CREATE_RESPONSE"
echo ""

# Step 2: Login to get JWT token
echo "Step 2: Logging in to get JWT token..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com"}' \
  --max-time 10)

echo "Response: $LOGIN_RESPONSE"

# Extract token from response (requires jq)
TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.token' 2>/dev/null)

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    echo "Failed to get token. Using mock token for demonstration."
    TOKEN="your-jwt-token-here"
fi

echo "Token: $TOKEN"
echo ""

# Step 3: Get current user information (Active User ID)
echo "Step 3: Getting current authenticated user information..."
echo "This demonstrates how to get the user ID of the user accessing the API"
USER_INFO=$(curl -s -X GET "$API_URL/user/me" \
  -H "Authorization: Bearer $TOKEN" \
  --max-time 10)

echo "Response: $USER_INFO"
echo ""

echo "=========================================="
echo "Test Complete!"
echo "=========================================="
echo ""
echo "The /user/me endpoint extracts the user ID from the JWT token"
echo "and returns the active user's information."
