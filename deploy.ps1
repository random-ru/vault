gcloud builds submit --tag gcr.io/ruler-rune/vault
gcloud run deploy vault --image gcr.io/ruler-rune/vault --platform managed --region europe-north1 --allow-unauthenticated