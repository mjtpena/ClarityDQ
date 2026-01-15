# ClarityDQ ML Service

Python-based machine learning service for anomaly detection in ClarityDQ.

## Technology Stack

- Python 3.11+
- Azure Machine Learning SDK
- scikit-learn for ML algorithms
- Prophet for time-series forecasting
- FastAPI for API endpoints
- pandas, numpy for data processing

## Features

### Anomaly Detection Types

1. **Volume Anomalies** - Unexpected row count changes
2. **Distribution Anomalies** - Unusual value distributions
3. **Pattern Anomalies** - New data patterns
4. **Timeliness Anomalies** - Late-arriving data

## ML Models

### Time-Series Forecasting
- Prophet for volume prediction
- ARIMA for trend analysis
- Seasonal decomposition

### Multivariate Anomaly Detection
- Isolation Forest
- One-Class SVM
- Local Outlier Factor (LOF)

### Pattern Learning
- Autoencoders for pattern detection
- LSTM for sequence anomalies

## Project Structure

```
ml-service/
├── models/              # Model definitions
├── training/            # Training scripts
├── inference/           # Inference services
├── api/                 # FastAPI endpoints
├── utils/               # Helper functions
├── tests/               # Unit tests
├── requirements.txt     # Dependencies
└── README.md
```

## Getting Started

### Prerequisites
- Python 3.11+
- Azure ML workspace
- Azure Storage account

### Installation

```bash
cd src/ml-service
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
```

### Configuration

Create `.env` file:

```bash
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZUREML_WORKSPACE_NAME=your-workspace-name
STORAGE_CONNECTION_STRING=your-storage-connection
```

### Development

```bash
# Run API server
uvicorn api.main:app --reload --port 8000

# Train models
python training/train_volume_anomaly.py

# Run tests
pytest tests/
```

## API Endpoints

### Anomaly Detection

```http
POST /api/v1/detect/volume
Content-Type: application/json

{
  "table_id": "table-uuid",
  "current_row_count": 1000000,
  "historical_data": [...]
}
```

Response:
```json
{
  "is_anomaly": true,
  "severity": "high",
  "confidence": 0.95,
  "expected_range": [900000, 1100000],
  "message": "Row count is 3.5 standard deviations from mean"
}
```

### Model Training

```http
POST /api/v1/train/volume-anomaly
Content-Type: application/json

{
  "table_id": "table-uuid",
  "training_days": 90
}
```

## Model Training

### Volume Anomaly Detection

```python
from training.volume_anomaly import VolumeAnomalyTrainer

trainer = VolumeAnomalyTrainer()
model = trainer.train(
    table_id="table-uuid",
    historical_data=df,
    algorithm="isolation_forest"
)
trainer.register_model(model, "volume-anomaly-v1")
```

### Distribution Anomaly Detection

```python
from training.distribution_anomaly import DistributionAnomalyTrainer

trainer = DistributionAnomalyTrainer()
model = trainer.train(
    column_profiles=profiles,
    algorithm="autoencoder"
)
```

## Inference

### Real-time Scoring

```python
from inference.anomaly_detector import AnomalyDetector

detector = AnomalyDetector.from_registry("volume-anomaly-v1")
result = detector.predict({
    "table_id": "table-uuid",
    "current_row_count": 1000000
})
```

### Batch Scoring

```python
from inference.batch_scorer import BatchScorer

scorer = BatchScorer()
results = scorer.score_batch(
    model_name="volume-anomaly-v1",
    input_data=df
)
```

## Model Registry

Models are versioned and stored in Azure ML Model Registry:

```python
from azureml.core import Model

# Register model
model = Model.register(
    workspace=ws,
    model_name="volume-anomaly-detector",
    model_path="./models/volume_anomaly.pkl",
    tags={"type": "anomaly-detection", "algorithm": "isolation-forest"},
    description="Detects volume anomalies in table row counts"
)
```

## Monitoring

Model performance is monitored using:
- Azure ML Model Monitoring
- Application Insights
- Custom metrics dashboard

### Key Metrics
- Precision, Recall, F1-score
- False positive rate
- Detection latency
- Model drift detection

## Testing

```bash
# Run unit tests
pytest tests/unit/

# Run integration tests
pytest tests/integration/

# Run with coverage
pytest --cov=. tests/
```

## Deployment

### Azure ML Endpoint

```python
from azureml.core.webservice import AciWebservice, Webservice

aci_config = AciWebservice.deploy_configuration(
    cpu_cores=2,
    memory_gb=4,
    auth_enabled=True
)

service = Model.deploy(
    workspace=ws,
    name="claritydq-anomaly-detector",
    models=[model],
    inference_config=inference_config,
    deployment_config=aci_config
)
```

### FastAPI Service

Deploy as containerized service to Azure Container Apps:

```bash
docker build -t claritydq-ml-service .
docker push acr.azurecr.io/claritydq-ml-service:latest
```

## Performance Optimization

- Model caching for faster inference
- Batch prediction for multiple tables
- Async processing with Celery
- GPU acceleration for deep learning models

## Continuous Training

Automated retraining pipeline:
1. Collect new training data weekly
2. Retrain models with updated data
3. Evaluate model performance
4. Register new model version if improved
5. Deploy to production endpoint

## Contributing

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines.

## References

- [Azure ML Documentation](https://docs.microsoft.com/azure/machine-learning/)
- [Prophet Documentation](https://facebook.github.io/prophet/)
- [scikit-learn Documentation](https://scikit-learn.org/)
