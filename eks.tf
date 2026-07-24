module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "~> 20.0"

  cluster_name    = var.cluster_name
  cluster_version = var.eks_version

  vpc_id     = aws_vpc.this.id
  subnet_ids = aws_subnet.private[*].id

  cluster_endpoint_public_access  = true
  cluster_endpoint_private_access = true

  enable_irsa = true

  cluster_enabled_log_types = var.enable_cluster_logs ? [
    "api",
    "audit",
    "authenticator",
    "controllerManager",
    "scheduler"
  ] : []

  cluster_addons = {
    coredns = {
      most_recent = true
    }
    kube-proxy = {
      most_recent = true
    }
    vpc-cni = {
      most_recent = true
    }
  }

  eks_managed_node_groups = {
    default = {
      desired_size = var.desired_capacity
      min_size     = var.min_size
      max_size     = var.max_size

      instance_types = var.node_instance_types
      capacity_type  = "ON_DEMAND"
      subnet_ids     = aws_subnet.private[*].id
      disk_size      = 20

      iam_role_additional_policies = {
        cloudwatch_agent = "arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy"
      }

      tags = {
        Name        = "${var.cluster_name}-default-ng"
        ManagedBy   = "ace"
        Environment = var.environment
        Project     = var.project_id
      }
    }
  }

  tags = merge(
    {
      ManagedBy   = "ace"
      Environment = var.environment
      Project     = var.project_id
    },
    var.tags,
  )
}

resource "aws_cloudwatch_log_group" "eks" {
  name              = "/aws/eks/${var.cluster_name}/cluster"
  retention_in_days = 30

  tags = merge(
    {
      Name        = "${var.cluster_name}-logs"
      ManagedBy   = "ace"
      Environment = var.environment
      Project     = var.project_id
    },
    var.tags,
  )
}