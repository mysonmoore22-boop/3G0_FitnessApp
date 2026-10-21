import statistics
import time
import httpx

RISK_URL = "http://127.0.0.1:8000/risk/evaluate"

# Ground Truth Dataset for Testing:
# is_attack: True = Attack Attempt (Expected Result: BLOCKED)
# is_attack: False = Legitimate User (Expected Result: ALLOWED)
TEST_CASES = [
    # Legitimate users
    {
        "expected_attack": False,
        "payload": {
            "identity_id": "trainer_dan",
            "target_action": "view_schedule",
            "client_ip": "10.0.0.12",
            "device_posture": {
                "is_disk_encrypted": True,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
        },
    },
    {
        "expected_attack": False,
        "payload": {
            "identity_id": "client_emma",
            "target_action": "view_client_data",
            "client_ip": "10.0.0.14",
            "device_posture": {
                "is_disk_encrypted": True,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
        },
    },
    # Scenario A: Compromised Trainer hitting Payouts
    {
        "expected_attack": True,
        "payload": {
            "identity_id": "trainer_sarah",
            "target_action": "access_payouts",
            "client_ip": "192.168.1.100",
            "device_posture": {
                "is_disk_encrypted": False,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
        },
    },
    # Scenario B: Stolen Session / Impossible Travel
    {
        "expected_attack": True,
        "payload": {
            "identity_id": "client_alex",
            "target_action": "view_client_data",
            "client_ip": "185.220.101.5",
            "device_posture": {
                "is_disk_encrypted": True,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
            "is_impossible_travel": True,
        },
    },
    # Scenario C: Admin Credential Abuse
    {
        "expected_attack": True,
        "payload": {
            "identity_id": "admin_root",
            "target_action": "admin_portal",
            "client_ip": "45.154.255.89",
            "device_posture": {
                "is_disk_encrypted": True,
                "os_version_supported": False,
                "edr_agent_active": False,
            },
            "failed_mfa_recent_count": 2,
        },
    },
]


def run_benchmark(iterations: int = 20):
    latencies = []
    tp, fp, tn, fn = 0, 0, 0, 0

    print(
        f"[*] Running Benchmark Harness ({len(TEST_CASES) * iterations} total evaluations)..."
    )

    with httpx.Client() as client:
        for _ in range(iterations):
            for test in TEST_CASES:
                t0 = time.perf_counter()
                resp = client.post(RISK_URL, json=test["payload"])
                dt = (time.perf_counter() - t0) * 1000
                latencies.append(dt)

                blocked = not resp.json()["action_permitted"]
                is_attack = test["expected_attack"]

                if is_attack and blocked:
                    tp += 1
                elif not is_attack and blocked:
                    fp += 1
                elif not is_attack and not blocked:
                    tn += 1
                elif is_attack and not blocked:
                    fn += 1

    total = tp + fp + tn + fn
    accuracy = (tp + tn) / total * 100
    precision = tp / (tp + fp) if (tp + fp) > 0 else 0
    recall = tp / (tp + fn) if (tp + fn) > 0 else 0

    print("\n" + "=" * 50)
    print("           EVALUATION METRICS & RESULTS           ")
    print("=" * 50)
    print(f"Total Requests Evaluated: {total}")
    print(f"Average Evaluation Latency: {statistics.mean(latencies):.2f} ms")
    print(
        f"p95 Latency:               {statistics.quantiles(latencies, n=20)[18]:.2f} ms"
    )
    print(f"p99 Latency:               {max(latencies):.2f} ms")
    print("-" * 50)
    print("Confusion Matrix:")
    print(f"  True Positives  (Attacks Blocked)        : {tp}")
    print(f"  True Negatives  (Legitimate Allowed)     : {tn}")
    print(f"  False Positives (Legitimate Blocked)     : {fp}")
    print(f"  False Negatives (Attacks Missed)         : {fn}")
    print("-" * 50)
    print(f"Accuracy:  {accuracy:.1f}%")
    print(f"Precision: {precision * 100:.1f}%")
    print(f"Recall:    {recall * 100:.1f}%")
    print("=" * 50)


if __name__ == "__main__":
    run_benchmark()
