# Delivery delay prediction mini-project

This learning project predicts whether a shipment will miss its promised delivery time.
It extends `Shipping.Partner.Integration`; it does not create orders or change shipment status.
The API remains protected by the existing shipping partner API-key middleware.

## Run the complete demo

From the repository root, with the .NET 10 SDK:

```bash
dotnet restore PartnerAPI.sln
dotnet run --project tools/Shipping.DelayPrediction.Trainer -- \
  data/delivery-delay.sample.csv \
  src/Shipping.Partner.Integration/Models/delivery-delay.zip delay-v1
dotnet test PartnerAPI.sln
dotnet run --project src/Shipping.Partner.Integration -- --urls http://localhost:5000
```

The trainer writes a model ZIP, `<model.zip>.json` (version, selected threshold,
SHA-256 binding), and `<model.zip>.metrics.json` (evaluation). Generated models are
ignored by git. Copy the ZIP and metadata together when deploying, configure
`DeliveryDelayPrediction:ModelPath` (environment variable
`DeliveryDelayPrediction__ModelPath`), and restart the API after replacing them.
Relative paths resolve against the API content root. The default is
`Models/delivery-delay.zip`. Models are loaded once; training never runs inside a request.
Only deploy artifacts produced by a trusted training process; the checksum detects mismatched files, not malicious artifacts.

Use an existing active partner API key from your configured credential store:

```bash
curl -X POST http://localhost:5000/shipping-orders/predict-delay \
  -H 'Content-Type: application/json' \
  -H "X-Shipping-Partner-Key: $PARTNER_API_KEY" \
  -d '{"distanceKm":8.5,"totalWeightKg":1.2,"serviceType":"standard","orderHour":18,"dayOfWeek":5,"activeOrdersAtCreation":24}'
```

Example response shape (values depend on the trained model):

```json
{"predictedLate":true,"delayProbability":0.78,"decisionThreshold":0.6,"modelVersion":"delay-v1"}
```

The repository's credential and order stores are in memory. Provision an active
credential using the existing integration setup; the prediction module does not
add a public credential bootstrap or bypass authentication. HTTP tests provision
a test-only validator to exercise 401/403 and authenticated predictions.

## Input contract and label

All six fields are required, including fields whose valid range includes zero.
No customer name, address, identifier, or outcome field is needed.

| Field | Accepted values |
| --- | --- |
| distanceKm | finite number greater than 0, at most 500 |
| totalWeightKg | finite number greater than 0, at most 1,000 |
| serviceType | `standard` or `express`, case sensitive |
| orderHour | integer 0–23 in the business timezone |
| dayOfWeek | integer 0–6, Sunday = 0 |
| activeOrdersAtCreation | integer 0–10,000, measured at prediction time |

`IsLate` means `actualDeliveredAt > promisedDeliveryAt` using the promise known at
creation, not a retrospectively revised promise. Late is the positive class.
Use completed shipments with reliable timestamps; define a separate policy for
cancellations and unresolved deliveries rather than silently labeling them on-time.
Never include actual delivery timestamps, final status, or future workload as features.
The current order/event records do not supply the complete training dataset: real
use needs collection of the creation-time feature snapshot and promised/actual delivery timestamps,
plus reliable shipment/order correlation. The sample CSV demonstrates the contract without changing those records.

Malformed/missing/out-of-range inputs return 400. Missing keys return 401; invalid
keys return 403. A missing, corrupt, mismatched, or incompatible model returns 503
for prediction while `/health` remains available. `/health` is process health, not model readiness.

## Dataset and experiment

`data/delivery-delay.sample.csv` contains **1,200 synthetic observations**, generated
with a fixed seed of 42 from simulated distance, weight, service type, time and
workload plus a stochastic label. It contains no real customer data. Its results
only show the pipeline works; they do not establish real shipping accuracy.
The file order represents creation order. For a real export, sort by creation
time before training; the trainer cannot independently verify chronology because
the CSV contains features and label only. Keep repeated observations of the same
shipment out of separate splits. The strict seven-column CSV format does not
support quoted fields; no field in this schema requires commas.

The trainer validates every row and requires at least 100 observations and both
classes in each split. Missing fields, unknown services and invalid labels fail
the run; they are not silently imputed or labeled on-time.

1. First 60%: fit one-hot encoding, numeric normalization and ML.NET SDCA logistic regression
   (seed 42, one training thread for repeatable experiments).
2. Next 20%: select a threshold from 0.05 to 0.95. Maximize late recall subject to
   precision >= 0.60, breaking ties by F1 then higher threshold. If infeasible,
   maximize F1 and explicitly report that the precision constraint was unmet.
3. Final 20%: evaluate the frozen model and selected threshold exactly once.
   Report precision, recall, F1, accuracy and TP/FP/FN/TN, plus an always-on-time baseline.

The report also includes Brier score and five probability calibration bins
(mean estimated probability versus observed late rate). `delayProbability` is a
logistic model estimate, not a guaranteed confidence level. Review calibration
on representative real data before using a value such as 0.78 as a business risk
estimate; any recalibration must be fitted outside the held-out test set.

## Architecture and verification

- `Application`: query, validation, result and `IDeliveryDelayPredictor` port; no ML.NET dependency.
- `Infrastructure`: model loading adapter, translates features into the ML contract.
- `Shipping.DelayPrediction`: shared ML schema, pipeline and model session.
- `tools/Shipping.DelayPrediction.Trainer`: offline dataset validation, chronological split,
  threshold selection, artifact export and evaluation.
- `Api`: HTTP contract and status mapping; reuses existing authentication.

Each request creates and disposes its own ML.NET PredictionEngine because engines
are not thread-safe. The immutable loaded transformer is shared. This is appropriate
for the mini-project; benchmark before introducing pooling for production traffic.

Tests train/save/reload a real model, run concurrent inference, verify threshold
and version metadata, detect checksum mismatch, exercise the HTTP endpoint with
real inference, require all fields, reject invalid inputs, check API-key behavior,
and verify 503 when the model is absent. CI additionally runs the full sample trainer.
