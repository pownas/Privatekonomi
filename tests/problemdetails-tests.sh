#!/bin/bash

# ProblemDetails Pattern Test Script
# Tests the ProblemDetails implementation for error handling

set -e

API_URL="${API_URL:-http://localhost:5277}"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "Testing ProblemDetails implementation at $API_URL"
echo "=================================================="
echo ""

# Function to test endpoint and check for ProblemDetails response
test_problem_details() {
    local method=$1
    local endpoint=$2
    local expected_status=$3
    local description=$4
    
    echo -n "Testing: $description... "
    
    # Make request and capture both status and response
    response=$(curl -s -w "\n%{http_code}" -X "$method" "$API_URL$endpoint" -H "Content-Type: application/json")
    
    # Split response body and status code
    status_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | head -n-1)
    
    if [ "$status_code" == "$expected_status" ]; then
        # Check if response contains ProblemDetails fields
        if echo "$body" | grep -q '"type"' && echo "$body" | grep -q '"title"' && echo "$body" | grep -q '"status"'; then
            echo -e "${GREEN}✓ PASS${NC} (HTTP $status_code, ProblemDetails format)"
            echo "  Response: $body" | head -c 100
            echo "..."
            return 0
        elif [ "$expected_status" == "200" ] || [ "$expected_status" == "201" ] || [ "$expected_status" == "204" ]; then
            # Success responses don't need ProblemDetails
            echo -e "${GREEN}✓ PASS${NC} (HTTP $status_code)"
            return 0
        else
            echo -e "${YELLOW}⚠ PARTIAL${NC} (HTTP $status_code, but not ProblemDetails format)"
            echo "  Response: $body"
            return 1
        fi
    else
        echo -e "${RED}✗ FAIL${NC} (Expected HTTP $expected_status, got HTTP $status_code)"
        echo "  Response: $body"
        return 1
    fi
}

# Test 404 - Resource Not Found
echo "Testing 404 Not Found Errors:"
test_problem_details "GET" "/api/categories/99999" "404" "GET non-existent category"
test_problem_details "GET" "/api/transactions/99999" "404" "GET non-existent transaction"
echo ""

# Test 400 - Bad Request
echo "Testing 400 Bad Request Errors:"
# Try to update with mismatched IDs
test_problem_details "PUT" "/api/categories/1" "400" "PUT category with mismatched ID" << EOF
{
    "categoryId": 999,
    "name": "Test Category",
    "type": 1
}
EOF

echo ""

# Test successful operations (should NOT return ProblemDetails)
echo "Testing Successful Operations (no ProblemDetails expected):"
test_problem_details "GET" "/api/categories" "200" "GET all categories"
test_problem_details "GET" "/api/transactions" "200" "GET all transactions"

echo ""
echo "=================================================="
echo -e "${GREEN}ProblemDetails tests completed!${NC}"
echo ""
echo "Note: Make sure the API is running before executing these tests."
echo "Start the API with: dotnet run --project src/Privatekonomi.Api/Privatekonomi.Api.csproj"
