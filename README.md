# 3G0 Fitness App: Zero Trust Policy & Risk Engine

A Zero Trust Architecture (ZTA) implementation adhering to **NIST SP 800-207** principles, combining an **ASP.NET Core protected application** behind an **OpenZiti** programmable zero-trust overlay network with an **Adaptive Risk Scoring Policy Engine** built in Python.

---

## 1. System Architecture & Split

The project decouples identity routing from application policy decisions:

* **Protected System & Overlay Network (C# / OpenZiti):**
  * Core application built in ASP.NET Core with role-based routing (`Trainer`, `Client`, `Admin`).
  * Deployed inside a private OpenZiti overlay network (Controller, Router, Edge Identities).
  * Enforces dynamic access revocation at both the network layer (Ziti edge routing) and application middleware layer.
* **Continuous Risk Engine & Testing Harness (Python / FastAPI):**
  * Implements a continuous **Policy Decision Point (PDP)** evaluating device posture, behavioral anomalies, and geo-velocity.
  * Translates raw security telemetry into a calibrated **5-Tier Trust/Risk Architecture**.
  * Contains automated adversarial attack simulations and an empirical benchmarking harness (latency overhead, classification accuracy, time-to-revoke).

---

## 2. Dynamic 5-Tier Trust Model

The Policy Engine assigns a dynamic score ($0 - 100$) evaluated against five operational trust bands:

| Trust Band | Score Range | Posture & Behavior Profile | Access Entitlements |
| :--- | :---: | :--- | :--- |
| **EXCELLENT** | $81 - 100$ | Encrypted disk, supported OS, active EDR, zero MFA faults | Full administrative and financial operations |
| **GOOD** | $61 - 80$ | Non-critical posture drift (e.g., minor OS patch lag) | Payouts, sensitive client data, routine schedules |
| **MODERATE** | $41 - 60$ | Posture defect (e.g., unencrypted disk, isolated MFA retry) | Client data and schedules (**payouts blocked**) |
| **POOR** | $21 - 40$ | Compound posture defects (e.g., missing EDR + unencrypted disk) | Read-only routine operations (`view_schedule` only) |
| **CRITICAL** | $0 - 20$ | Impossible travel velocity, credential abuse, or brute force | **Total quarantine** (all endpoints return `403 Forbidden`) |

---

## 3. Integration Contract: `POST /risk/evaluate`

The Policy Decision Point exposes a standardized REST contract consumed by the C# application layer and OpenZiti management webhooks.

* **Endpoint:** `POST http://127.0.0.1:8000/risk/evaluate`
* **Interactive Docs:** `http://127.0.0.1:8000/docs`

### Request Payload Format
```json
{
  "identity_id": "trainer_sarah",
  "target_action": "access_payouts",
  "client_ip": "192.168.1.100",
  "device_posture": {
    "is_disk_encrypted": false,
    "os_version_supported": true,
    "edr_agent_active": true
  },
  "is_impossible_travel": false,
  "failed_mfa_recent_count": 0
}
