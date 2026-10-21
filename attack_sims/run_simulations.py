import time
import httpx

RISK_URL = "http://127.0.0.1:8000/risk/evaluate"


def evaluate_action(test_name: str, payload: dict):
    start = time.perf_counter()
    resp = httpx.post(RISK_URL, json=payload)
    latency_ms = (time.perf_counter() - start) * 1000
    data = resp.json()

    print(f"\n[{test_name}]")
    print(f"  Target:  {payload['target_action']} | Identity: {payload['identity_id']}")
    print(f"  Trust:   {data['trust_score']}/100 | Tier: {data['trust_tier']}")
    print(f"  Outcome: {'ALLOWED' if data['action_permitted'] else 'BLOCKED'}")
    print(f"  Reasons: {', '.join(data['reasons'])}")
    print(f"  Latency: {latency_ms:.2f} ms")


def run():
    print("=" * 60)
    print("RUNNING 5-TIER ADAPTIVE ZERO TRUST SIMULATION")
    print("=" * 60)

    # 1. Clean Trainer
    evaluate_action(
        "TEST 1: Clean Trainer accessing Payouts (Expected: EXCELLENT -> ALLOWED)",
        {
            "identity_id": "trainer_sarah",
            "target_action": "access_payouts",
            "client_ip": "10.0.0.5",
            "device_posture": {
                "is_disk_encrypted": True,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
        },
    )

    # 2. Compromised Trainer (Disk not encrypted -> -30 -> Trust: 70, MODERATE)
    evaluate_action(
        "TEST 2A: Compromised Trainer viewing Schedule (Expected: MODERATE -> ALLOWED)",
        {
            "identity_id": "trainer_sarah",
            "target_action": "view_schedule",
            "client_ip": "10.0.0.5",
            "device_posture": {
                "is_disk_encrypted": False,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
        },
    )

    evaluate_action(
        "TEST 2B: Compromised Trainer accessing Payouts (Expected: MODERATE -> BLOCKED)",
        {
            "identity_id": "trainer_sarah",
            "target_action": "access_payouts",
            "client_ip": "10.0.0.5",
            "device_posture": {
                "is_disk_encrypted": False,
                "os_version_supported": True,
                "edr_agent_active": True,
            },
        },
    )

    # 3. Stolen Session (Impossible Travel -> -80 -> Trust: 20, CRITICAL)
    evaluate_action(
        "TEST 3: Stolen Session / Impossible Travel (Expected: CRITICAL -> BLOCKED)",
        {
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
    )


if __name__ == "__main__":
    run()
