import urllib.request
import urllib.parse
import json
import time
import random
import string
import ssl
from concurrent.futures import ThreadPoolExecutor

# Disable SSL verification for local testing
ssl_context = ssl.create_default_context()
ssl_context.check_hostname = False
ssl_context.verify_mode = ssl.CERT_NONE

def generate_random_email():
    """Generate random email address"""
    username = ''.join(random.choices(string.ascii_lowercase + string.digits, k=10))
    domain = ''.join(random.choices(string.ascii_lowercase, k=5))  # Fixed this line
    return f"{username}@{domain}.com"

def send_signup_request(request_num):
    """Send a single signup request using urllib"""
    url = "https://localhost:7077/user/sign-up"
    
    payload = {
        "password": "password",
        "email": generate_random_email(),
        "isMfaEnabled": False
    }
    
    json_data = json.dumps(payload).encode('utf-8')
    
    headers = {
        'Content-Type': 'application/json'
    }
    
    try:
        req = urllib.request.Request(
            url, 
            data=json_data, 
            headers=headers,
            method='POST'
        )
        
        response = urllib.request.urlopen(req, context=ssl_context)
        print(f"Request {request_num}: Status {response.status}, Email: {payload['email']}")
        return response.status
        
    except Exception as e:
        print(f"Request {request_num}: Failed - {e}")
        return None

def run_load_test(requests_per_second=100, duration_seconds=10):
    """Run load test with specified RPS for given duration"""
    print(f"Starting load test: {requests_per_second} requests/second for {duration_seconds} seconds")
    print("=" * 60)
    
    total_requests = requests_per_second * duration_seconds
    request_count = 0
    start_time = time.time()
    
    with ThreadPoolExecutor(max_workers=50) as executor:
        for second in range(duration_seconds):
            second_start = time.time()
            
            # Submit requests for current second
            futures = []
            for i in range(requests_per_second):
                future = executor.submit(send_signup_request, request_count)
                futures.append(future)
                request_count += 1
            
            # Wait until the end of the current second
            elapsed = time.time() - second_start
            if elapsed < 1.0:
                time.sleep(1.0 - elapsed)
            
            print(f"Second {second + 1}: Sent {requests_per_second} requests")
    
    total_time = time.time() - start_time
    print("=" * 60)
    print(f"Load test completed in {total_time:.2f} seconds")
    print(f"Total requests sent: {total_requests}")

if __name__ == "__main__":
    run_load_test(requests_per_second=2, duration_seconds=600)