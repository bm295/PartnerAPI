# Shipment Status Lifecycle

Shipment events move through a constrained lifecycle. The current domain model supports eight statuses and a transition map. Terminal statuses cannot transition to any other status.

## Statuses

| Status | Meaning | Terminal |
| --- | --- | --- |
| `LabelCreated` | A label or shipment record exists, but the carrier has not picked up the parcel. | No |
| `PickedUp` | Carrier or courier accepted the parcel. | No |
| `InTransit` | Parcel is moving through the carrier network. | No |
| `OutForDelivery` | Parcel is with the delivery agent for final delivery. | No |
| `Delivered` | Parcel was delivered. | Yes |
| `Exception` | Carrier reported an issue such as delay, address problem, customs hold, or damage. | No |
| `Returned` | Parcel has been returned to sender or return flow is complete. | Yes |
| `Cancelled` | Shipment was cancelled before completion. | Yes |

## Valid transitions

| From | Allowed next statuses | Examples |
| --- | --- | --- |
| `LabelCreated` | `PickedUp`, `Cancelled`, `Exception` | Label created then pickup scan; label cancelled before pickup; label has an address exception. |
| `PickedUp` | `InTransit`, `Exception`, `Returned`, `Cancelled` | Pickup moves to linehaul; package damaged after pickup; merchant requests return; pickup is voided. |
| `InTransit` | `OutForDelivery`, `Exception`, `Returned` | Parcel arrives at destination depot; customs hold occurs; carrier starts return-to-sender. |
| `OutForDelivery` | `Delivered`, `Exception`, `Returned` | Driver delivers; delivery attempt fails; package is returned after failed attempts. |
| `Delivered` | None | Delivery is terminal. Corrections require support tooling, not a normal transition. |
| `Exception` | `InTransit`, `OutForDelivery`, `Delivered`, `Returned`, `Cancelled` | Issue is resolved and parcel moves; parcel is still delivered; parcel returns; shipment is cancelled. |
| `Returned` | None | Return is terminal. |
| `Cancelled` | None | Cancellation is terminal. |

## Invalid transition examples

| Previous status | Rejected next status | Reason |
| --- | --- | --- |
| `Delivered` | `InTransit` | Delivered is terminal. |
| `Returned` | `Delivered` | Returned is terminal. |
| `Cancelled` | `PickedUp` | Cancelled is terminal. |
| `LabelCreated` | `Delivered` | Shipment must be picked up or have an exception path before delivery. |
| `InTransit` | `PickedUp` | Lifecycle cannot move backward to pickup. |

## Current API behavior

- `POST /shipments/events` appends an event for a known partner.
- The event store looks up the latest event for the same partner and tracking number by `occurredAtUtc` and checks whether the requested transition is allowed.
- Invalid transitions currently throw an exception instead of returning the stable `INVALID_STATUS_TRANSITION` error. This should be converted to a `400 Bad Request` before public launch.
- If there is no previous event for a partner/tracking number, the current implementation accepts any valid status as the first event. Public launch should decide whether first events must start at `LabelCreated` or may begin with any status imported from a carrier.
