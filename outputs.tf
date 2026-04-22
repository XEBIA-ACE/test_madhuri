output "cluster_id" {
  description = "EKS cluster ID"
  value       = aws_eks_cluster.main.id
}

output "cluster_arn" {
  description = "EKS cluster ARN"
  value       = aws_eks_cluster.main.arn
}

output "cluster_endpoint" {
  description = "Endpoint for EKS control plane"
  value       = aws_eks_cluster.main.endpoint
}

output "cluster_security_group_id" {
  description = "Security group ids attached to the cluster control plane"
  value       = aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
}

output "cluster_certificate_authority_data" {
  description = "Base64 encoded certificate data required to communicate with the cluster"
  value       = aws_eks_cluster.main.certificate_authority[0].data
}

output "cluster_version" {
  description = "The Kubernetes version for the EKS cluster"
  value       = aws_eks_cluster.main.version
}

output "node_group_arn" {
  description = "Amazon Resource Name (ARN) of the EKS Node Group"
  value       = aws_eks_node_group.main.arn
}

output "node_group_status" {
  description = "Status of the EKS Node Group"
  value       = aws_eks_node_group.main.status
}

output "vpc_id" {
  description = "ID of the VPC where the cluster is deployed"
  value       = aws_vpc.main.id
}

output "subnet_ids" {
  description = "IDs of the subnets where the cluster is deployed"
  value       = aws_subnet.public[*].id
}

## Deployment Instructions

1. **Initialize Terraform:**
   ```bash
   terraform init
   ```

2. **Review the plan:**
   ```bash
   terraform plan
   ```

3. **Apply the configuration:**
   ```bash
   terraform apply
   ```

4. **Configure kubectl:**
   ```bash
   aws eks update-kubeconfig --region us-east-1 --name my-eks-cluster
   ```

5. **Verify the cluster:**
   ```bash
   kubectl get nodes
   kubectl get pods -A
   ```

## Configuration Features

- **Minimal setup**: Basic EKS cluster with managed node groups
- **Security**: IAM roles with least privilege, security groups configured
- **Networking**: Public subnets only (suitable for development)
- **Logging**: Control plane logging enabled
- **Add-ons**: Essential EKS add-ons (VPC CNI, CoreDNS, kube-proxy, EBS CSI driver)
- **Scalability**: Auto-scaling node group (1-4 nodes)

## Customization

To customize the cluster, modify the variables in `terraform.tfvars`:

```hcl
cluster_name = "my-production-cluster"
environment = "prod"
node_instance_types = ["t3.large"]
node_desired_capacity = 3
node_max_capacity = 10
```

This minimal configuration provides a production-ready EKS cluster with essential security and operational features while keeping complexity low for development environments.