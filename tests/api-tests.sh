#!/bin/bash

# API Endpoint Test Script
# Tests the new API endpoints added based on OpenAPI specification

set -e

API_URL="${API_URL:-http://localhost:5277}"
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo "Testing API endpoints at $API_URL"
echo "=================================="
echo ""

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local expected_status=$3
    local description=$4
    
    echo -n "Testing: $description... "
    
    status_code=$(curl -s -o /dev/null -w "%{http_code}" -X "$method" "$API_URL$endpoint")
    
    if [ "$status_code" == "$expected_status" ]; then
        echo -e "${GREEN}✓ PASS${NC} (HTTP $status_code)"
        return 0
    else
        echo -e "${RED}✗ FAIL${NC} (Expected HTTP $expected_status, got HTTP $status_code)"
        return 1
    fi
}

# Test Accounts Controller
echo "Testing Accounts Controller:"
test_endpoint "GET" "/api/accounts" "200" "GET /api/accounts"

# Test Reports Controller
echo ""
echo "Testing Reports Controller:"
test_endpoint "GET" "/api/reports/networth" "200" "GET /api/reports/networth"
test_endpoint "GET" "/api/reports/summary" "200" "GET /api/reports/summary"
test_endpoint "GET" "/api/reports/summary?year=2024&month=10" "200" "GET /api/reports/summary with params"

# Test Goals Controller
echo ""
echo "Testing Goals Controller:"
test_endpoint "GET" "/api/goals" "200" "GET /api/goals"

# Test Enhanced Transactions Controller
echo ""
echo "Testing Enhanced Transactions Controller:"
test_endpoint "GET" "/api/transactions" "200" "GET /api/transactions"
test_endpoint "GET" "/api/transactions?page=1&per_page=10" "200" "GET /api/transactions with pagination"
test_endpoint "GET" "/api/transactions?account_id=1" "200" "GET /api/transactions with account filter"

# Test Enhanced Budgets Controller
echo ""
echo "Testing Enhanced Budgets Controller:"
test_endpoint "GET" "/api/budgets" "200" "GET /api/budgets"
test_endpoint "GET" "/api/budgets?period_start=2024-01-01&period_end=2024-12-31" "200" "GET /api/budgets with period filter"

# Test existing controllers still work
echo ""
echo "Testing Existing Controllers (backward compatibility):"
test_endpoint "GET" "/api/categories" "200" "GET /api/categories"
test_endpoint "GET" "/api/loans" "200" "GET /api/loans"

echo ""
echo "=================================="
echo -e "${GREEN}All tests completed!${NC}"
