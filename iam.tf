data "aws_iam_policy_document" "cluster_autoscaler" {
  statement {
    sid    = "ClusterAutoscalerAll"
    effect = "Allow"

    actions = [
      "autoscaling:DescribeAutoScalingGroups",
      "autoscaling:DescribeAutoScalingInstances",
      "autoscaling:DescribeLaunchConfigurations",
      "autoscaling:DescribeTags",
      "autoscaling:SetDesiredCapacity",
      "autoscaling:TerminateInstanceInAutoScalingGroup",
      "ec2:DescribeLaunchTemplateVersions"
    ]

    resources = ["*"]
  }
}

resource "aws_iam_policy" "cluster_autoscaler" {
  name        = "${var.cluster_name}-cluster-autoscaler"
  description = "EKS cluster-autoscaler policy"
  policy      = data.aws_iam_policy_document.cluster_autoscaler.json

  tags = merge(
    {
      ManagedBy   = "ace"
      Environment = var.environment
      Project     = var.project_id
    },
    var.tags,
  )
}

resource "aws_iam_service_linked_role" "eks" {
  aws_service_name = "eks.amazonaws.com"
}