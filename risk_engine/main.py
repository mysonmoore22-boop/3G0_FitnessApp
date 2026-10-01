from enum import Enum
from typing import Optional
from fastapi import FastAPI
from pydantic import BaseModel, Field

app = FastAPI(title="Zero Trust Risk Scoring Engine", version="1.0.0")


class RiskTier(str, Enum):
    LOW = "LOW"
    MEDIUM = "MEDIUM"
    HIGH = "HIGH"


class DevicePosture(BaseModel):
    is_disk_encrypted: bool
    os_version_supported: bool
    edr_agent_active: bool


class EvaluationRequest(BaseModel):
    identity_id: str
    target_action: str  # e.g., "view_schedule", "access_payouts", "admin_portal"
    client_ip: str
    device_posture: DevicePosture
    is_impossible_travel: bool = False
    failed_mfa_recent_count: int = Field(default=0, ge=0)


class EvaluationResponse(BaseModel):
    identity_id: str
    risk_score: int
    risk_tier: RiskTier
    action_permitted: bool
    reason: str


@app.post("/risk/evaluate", response_model=EvaluationResponse)
async def evaluate_risk(payload: EvaluationRequest) -> EvaluationResponse:
    score = 0
    reasons = []

    # 1. Posture evaluations (Baseline checks)
    if not payload.device_posture.is_disk_encrypted:
        score += 35
        reasons.append("Unencrypted disk detected")

    if not payload.device_posture.edr_agent_active:
        score += 30
        reasons.append("EDR agent missing or disabled")

    if not payload.device_posture.os_version_supported:
        score += 15
        reasons.append("Outdated OS version")

    # 2. Network & behavioral flags
    if payload.is_impossible_travel:
        score += 50
        reasons.append("Impossible travel velocity observed")

    if payload.failed_mfa_recent_count > 0:
        score += min(payload.failed_mfa_recent_count * 15, 45)
        reasons.append(f"{payload.failed_mfa_recent_count} recent failed MFA attempts")

    # Cap score at 100
    score = min(score, 100)

    # 3. Categorize Risk Tier
    if score >= 60:
        tier = RiskTier.HIGH
    elif score >= 30:
        tier = RiskTier.MEDIUM
    else:
        tier = RiskTier.LOW

    # 4. Zero Trust Policy Decision:
    # Key scenario requirement: High-value actions (payouts/admin) require LOW risk.
    # Routine actions (schedule) remain permitted even under MEDIUM risk.
    if tier == RiskTier.HIGH:
        permitted = False
    elif tier == RiskTier.MEDIUM and payload.target_action in [
        "access_payouts",
        "admin_portal",
    ]:
        permitted = False
    else:
        permitted = True

    return EvaluationResponse(
        identity_id=payload.identity_id,
        risk_score=score,
        risk_tier=tier,
        action_permitted=permitted,
        reason=", ".join(reasons) if reasons else "Nominal telemetry parameters",
    )
