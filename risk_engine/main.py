from enum import Enum
from typing import List
from fastapi import FastAPI
from pydantic import BaseModel, Field

app = FastAPI(title="Zero Trust 5-Tier Adaptive Policy Engine", version="2.0.0")


class TrustTier(str, Enum):
    CRITICAL = "CRITICAL"  # Terrible (0 - 20)
    POOR = "POOR"  # Poor (21 - 40)
    MODERATE = "MODERATE"  # Moderate (41 - 60)
    GOOD = "GOOD"  # Good (61 - 80)
    EXCELLENT = "EXCELLENT"  # Great (81 - 100)


class DevicePosture(BaseModel):
    is_disk_encrypted: bool
    os_version_supported: bool
    edr_agent_active: bool


class EvaluationRequest(BaseModel):
    identity_id: str
    target_action: (
        str  # "admin_portal", "access_payouts", "view_client_data", "view_schedule"
    )
    client_ip: str
    device_posture: DevicePosture
    is_impossible_travel: bool = False
    failed_mfa_recent_count: int = Field(default=0, ge=0)


class EvaluationResponse(BaseModel):
    identity_id: str
    trust_score: int
    risk_score: int
    trust_tier: TrustTier
    action_permitted: bool
    reasons: List[str]


@app.post("/risk/evaluate", response_model=EvaluationResponse)
async def evaluate_risk(payload: EvaluationRequest) -> EvaluationResponse:
    # Baseline: Start with perfect trust (100) and zero risk (0)
    trust = 100
    reasons: List[str] = []

    # 1. Posture Penalties
    if not payload.device_posture.is_disk_encrypted:
        trust -= 30
        reasons.append("Unencrypted storage detected (-30)")

    if not payload.device_posture.edr_agent_active:
        trust -= 35
        reasons.append("EDR / endpoint protection disabled (-35)")

    if not payload.device_posture.os_version_supported:
        trust -= 15
        reasons.append("Outdated or unsupported OS (-15)")

    # 2. Behavioral & Velocity Penalties
    if payload.is_impossible_travel:
        trust -= 80  # Severe indicator of token theft / proxy hopping
        reasons.append("Impossible travel velocity observed (-80)")

    if payload.failed_mfa_recent_count > 0:
        penalty = min(payload.failed_mfa_recent_count * 20, 60)
        trust -= penalty
        reasons.append(
            f"{payload.failed_mfa_recent_count} recent MFA failures (-{penalty})"
        )

    # Clamp score between 0 and 100
    trust_score = max(0, min(trust, 100))
    risk_score = 100 - trust_score

    # 3. Categorize into 5 equal 20-point tiers
    if trust_score > 80:
        tier = TrustTier.EXCELLENT
    elif trust_score > 60:
        tier = TrustTier.GOOD
    elif trust_score > 40:
        tier = TrustTier.MODERATE
    elif trust_score > 20:
        tier = TrustTier.POOR
    else:
        tier = TrustTier.CRITICAL

    # 4. Action Access Matrix (Continuous Least-Privilege Verification)
    action = payload.target_action
    permitted = False

    if tier == TrustTier.EXCELLENT:
        permitted = True
    elif tier == TrustTier.GOOD:
        permitted = action in ["access_payouts", "view_client_data", "view_schedule"]
    elif tier == TrustTier.MODERATE:
        # Compromised trainer scenario: unencrypted disk drops trust here.
        # Can still view data and schedule, but payouts are locked out.
        permitted = action in ["view_client_data", "view_schedule"]
    elif tier == TrustTier.POOR:
        # Device severely neglected: read-only non-sensitive operations only.
        permitted = action in ["view_schedule"]
    else:  # CRITICAL (Terrible)
        # Impossible travel or major breach indicator: complete quarantine.
        permitted = False

    if not reasons:
        reasons.append("Telemetry verified nominal")

    return EvaluationResponse(
        identity_id=payload.identity_id,
        trust_score=trust_score,
        risk_score=risk_score,
        trust_tier=tier,
        action_permitted=permitted,
        reasons=reasons,
    )
