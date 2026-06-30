using TorchSharp;

namespace ObstacleCourse
{
    public class PPOTrainer
    {
        const float gamma = 0.99f;
        const float gae_lambda = 0.95f;
        const float clip_epsilon = 0.2f;
        const float entropy_coeff = 0.01f;
        const float value_coeff = 0.5f;
        const int ppo_epochs = 4;
        const int mini_batch_size = 256;
        const float max_grad_norm = 0.5f;

        Brain brain;

        public PPOTrainer(Brain brain)
        {
            this.brain = brain;
        }

        public void train(List<Agent> agents)
        {
            var all_states = new List<float[]>();
            var all_actions = new List<float[]>();
            var all_log_probs = new List<float>();
            var all_advantages = new List<float>();
            var all_returns = new List<float>();

            foreach (Agent a in agents)
            {
                if (a.trajectory.rewards.Count == 0) continue;

                var (advantages, returns) = compute_gae(a.trajectory);
                all_states.AddRange(a.trajectory.states);
                all_actions.AddRange(a.trajectory.actions);
                all_log_probs.AddRange(a.trajectory.log_probs);
                all_advantages.AddRange(advantages);
                all_returns.AddRange(returns);
            }

            int n = all_states.Count;
            if (n == 0) return;

            int num_inputs = 26;
            int num_actions = 2;

            var states_tensor = torch.zeros(n, num_inputs);
            var actions_tensor = torch.zeros(n, num_actions);
            var old_log_probs_tensor = torch.zeros(n);
            var advantages_tensor = torch.zeros(n);
            var returns_tensor = torch.zeros(n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < num_inputs; j++)
                    states_tensor[i, j] = torch.tensor(all_states[i][j]);
                for (int j = 0; j < num_actions; j++)
                    actions_tensor[i, j] = torch.tensor(all_actions[i][j]);
                old_log_probs_tensor[i] = torch.tensor(all_log_probs[i]);
                advantages_tensor[i] = torch.tensor(all_advantages[i]);
                returns_tensor[i] = torch.tensor(all_returns[i]);
            }

            float adv_mean = advantages_tensor.mean().item<float>();
            float adv_std = advantages_tensor.std().item<float>() + 1e-8f;
            advantages_tensor = (advantages_tensor - adv_mean) / adv_std;

            for (int epoch = 0; epoch < ppo_epochs; epoch++)
            {
                var indices = torch.randperm(n);

                for (int start = 0; start < n; start += mini_batch_size)
                {
                    int end = Math.Min(start + mini_batch_size, n);
                    var idx = indices.slice(0, start, end, 1);

                    var mb_states = states_tensor.index_select(0, idx);
                    var mb_actions = actions_tensor.index_select(0, idx);
                    var mb_old_log_probs = old_log_probs_tensor.index_select(0, idx);
                    var mb_advantages = advantages_tensor.index_select(0, idx);
                    var mb_returns = returns_tensor.index_select(0, idx);

                    var (new_log_probs, values, entropy) = brain.evaluate(mb_states, mb_actions);

                    var ratio = (new_log_probs - mb_old_log_probs).exp();
                    var surr1 = ratio * mb_advantages;
                    var surr2 = ratio.clamp(1.0 - clip_epsilon, 1.0 + clip_epsilon) * mb_advantages;
                    var policy_loss = -torch.min(surr1, surr2).mean();

                    var value_loss = torch.nn.functional.mse_loss(values, mb_returns);

                    var entropy_loss = -entropy.mean();

                    var total_loss = policy_loss + value_coeff * value_loss + entropy_coeff * entropy_loss;

                    brain.optimizer.zero_grad();
                    total_loss.backward();
                    torch.nn.utils.clip_grad_norm_(brain.parameters(), max_grad_norm);
                    brain.optimizer.step();
                }
            }
        }

        (float[] advantages, float[] returns) compute_gae(Trajectory traj)
        {
            int T = traj.rewards.Count;
            float[] advantages = new float[T];
            float[] returns = new float[T];

            float gae = 0;
            float next_value = 0;

            for (int t = T - 1; t >= 0; t--)
            {
                float delta = traj.rewards[t] + gamma * next_value - traj.values[t];
                gae = delta + gamma * gae_lambda * gae;
                advantages[t] = gae;
                returns[t] = gae + traj.values[t];
                next_value = traj.values[t];
            }

            return (advantages, returns);
        }
    }
}
