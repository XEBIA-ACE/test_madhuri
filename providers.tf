terraform {
  required_version = ">= 1.5.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.41"
    }
  }
  backend "s3" {
    bucket         = "eks-tfstate-bucket-dev"
    key            = "eks/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "eks-tfstate-lock"
  }
}

provider "aws" {
  region = var.region
  default_tags {
    tags = {
      Environment = var.environment
      Project     = var.project_name
      ManagedBy   = "Terraform"
      Owner       = "DevOps"
    }
  }
}