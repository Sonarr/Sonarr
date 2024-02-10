using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.LibTorrent.Models
{
    /// <summary> Re-Implements LibTorrent <a href="https://libtorrent.org/reference-Settings.html#settings_pack">settings_pack</a></summary>
    public class LibTorrentSettingsPack
    {
        [JsonProperty("active_checking", NullValueHandling = NullValueHandling.Ignore)]
        public int active_checking { get; set; }

        [JsonProperty(nameof(active_dht_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int active_dht_limit { get; set; }

        [JsonProperty(nameof(active_downloads), NullValueHandling = NullValueHandling.Ignore)]
        public int active_downloads { get; set; }

        [JsonProperty(nameof(active_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int active_limit { get; set; }

        [JsonProperty(nameof(active_lsd_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int active_lsd_limit { get; set; }

        [JsonProperty(nameof(active_seeds), NullValueHandling = NullValueHandling.Ignore)]
        public int active_seeds { get; set; }

        [JsonProperty(nameof(active_tracker_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int active_tracker_limit { get; set; }

        [JsonProperty(nameof(aio_threads), NullValueHandling = NullValueHandling.Ignore)]
        public int aio_threads { get; set; }

        [JsonProperty(nameof(alert_mask), NullValueHandling = NullValueHandling.Ignore)]
        public int alert_mask { get; set; }

        [JsonProperty(nameof(alert_queue_size), NullValueHandling = NullValueHandling.Ignore)]
        public int alert_queue_size { get; set; }

        [JsonProperty(nameof(allow_i2p_mixed), NullValueHandling = NullValueHandling.Ignore)]
        public bool allow_i2p_mixed { get; set; }

        [JsonProperty(nameof(allow_idna), NullValueHandling = NullValueHandling.Ignore)]
        public bool allow_idna { get; set; }

        [JsonProperty(nameof(allow_multiple_connections_per_ip), NullValueHandling = NullValueHandling.Ignore)]
        public bool allow_multiple_connections_per_ip { get; set; }

        [JsonProperty(nameof(allowed_enc_level), NullValueHandling = NullValueHandling.Ignore)]
        public int allowed_enc_level { get; set; }

        [JsonProperty(nameof(allowed_fast_set_size), NullValueHandling = NullValueHandling.Ignore)]
        public int allowed_fast_set_size { get; set; }

        [JsonProperty(nameof(always_send_user_agent), NullValueHandling = NullValueHandling.Ignore)]
        public bool always_send_user_agent { get; set; }

        [JsonProperty(nameof(announce_crypto_support), NullValueHandling = NullValueHandling.Ignore)]
        public bool announce_crypto_support { get; set; }

        [JsonProperty(nameof(announce_ip), NullValueHandling = NullValueHandling.Ignore)]
        public string announce_ip { get; set; }

        [JsonProperty(nameof(announce_to_all_tiers), NullValueHandling = NullValueHandling.Ignore)]
        public bool announce_to_all_tiers { get; set; }

        [JsonProperty(nameof(announce_to_all_trackers), NullValueHandling = NullValueHandling.Ignore)]
        public bool announce_to_all_trackers { get; set; }

        [JsonProperty(nameof(anonymous_mode), NullValueHandling = NullValueHandling.Ignore)]
        public bool anonymous_mode { get; set; }

        [JsonProperty(nameof(apply_ip_filter_to_trackers), NullValueHandling = NullValueHandling.Ignore)]
        public bool apply_ip_filter_to_trackers { get; set; }

        [JsonProperty(nameof(auto_manage_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int auto_manage_interval { get; set; }

        [JsonProperty(nameof(auto_manage_prefer_seeds), NullValueHandling = NullValueHandling.Ignore)]
        public bool auto_manage_prefer_seeds { get; set; }

        [JsonProperty(nameof(auto_manage_startup), NullValueHandling = NullValueHandling.Ignore)]
        public int auto_manage_startup { get; set; }

        [JsonProperty(nameof(auto_scrape_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int auto_scrape_interval { get; set; }

        [JsonProperty(nameof(auto_scrape_min_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int auto_scrape_min_interval { get; set; }

        [JsonProperty(nameof(auto_sequential), NullValueHandling = NullValueHandling.Ignore)]
        public bool auto_sequential { get; set; }

        [JsonProperty(nameof(ban_web_seeds), NullValueHandling = NullValueHandling.Ignore)]
        public bool ban_web_seeds { get; set; }

        [JsonProperty(nameof(checking_mem_usage), NullValueHandling = NullValueHandling.Ignore)]
        public int checking_mem_usage { get; set; }

        [JsonProperty(nameof(choking_algorithm), NullValueHandling = NullValueHandling.Ignore)]
        public int choking_algorithm { get; set; }

        [JsonProperty(nameof(close_file_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int close_file_interval { get; set; }

        [JsonProperty(nameof(close_redundant_connections), NullValueHandling = NullValueHandling.Ignore)]
        public bool close_redundant_connections { get; set; }

        [JsonProperty(nameof(connect_seed_every_n_download), NullValueHandling = NullValueHandling.Ignore)]
        public int connect_seed_every_n_download { get; set; }

        [JsonProperty(nameof(connection_speed), NullValueHandling = NullValueHandling.Ignore)]
        public int connection_speed { get; set; }

        [JsonProperty(nameof(connections_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int connections_limit { get; set; }

        [JsonProperty(nameof(connections_slack), NullValueHandling = NullValueHandling.Ignore)]
        public int connections_slack { get; set; }

        [JsonProperty(nameof(dht_aggressive_lookups), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_aggressive_lookups { get; set; }

        [JsonProperty(nameof(dht_announce_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_announce_interval { get; set; }

        [JsonProperty(nameof(dht_block_ratelimit), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_block_ratelimit { get; set; }

        [JsonProperty(nameof(dht_block_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_block_timeout { get; set; }

        [JsonProperty(nameof(dht_bootstrap_nodes), NullValueHandling = NullValueHandling.Ignore)]
        public string dht_bootstrap_nodes { get; set; }

        [JsonProperty(nameof(dht_enforce_node_id), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_enforce_node_id { get; set; }

        [JsonProperty(nameof(dht_extended_routing_table), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_extended_routing_table { get; set; }

        [JsonProperty(nameof(dht_ignore_dark_internet), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_ignore_dark_internet { get; set; }

        [JsonProperty(nameof(dht_item_lifetime), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_item_lifetime { get; set; }

        [JsonProperty(nameof(dht_max_dht_items), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_dht_items { get; set; }

        [JsonProperty(nameof(dht_max_fail_count), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_fail_count { get; set; }

        [JsonProperty(nameof(dht_max_infohashes_sample_count), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_infohashes_sample_count { get; set; }

        [JsonProperty(nameof(dht_max_peers), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_peers { get; set; }

        [JsonProperty(nameof(dht_max_peers_reply), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_peers_reply { get; set; }

        [JsonProperty(nameof(dht_max_torrent_search_reply), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_torrent_search_reply { get; set; }

        [JsonProperty(nameof(dht_max_torrents), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_max_torrents { get; set; }

        [JsonProperty(nameof(dht_prefer_verified_node_ids), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_prefer_verified_node_ids { get; set; }

        [JsonProperty(nameof(dht_privacy_lookups), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_privacy_lookups { get; set; }

        [JsonProperty(nameof(dht_read_only), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_read_only { get; set; }

        [JsonProperty(nameof(dht_restrict_routing_ips), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_restrict_routing_ips { get; set; }

        [JsonProperty(nameof(dht_restrict_search_ips), NullValueHandling = NullValueHandling.Ignore)]
        public bool dht_restrict_search_ips { get; set; }

        [JsonProperty(nameof(dht_sample_infohashes_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_sample_infohashes_interval { get; set; }

        [JsonProperty(nameof(dht_search_branching), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_search_branching { get; set; }

        [JsonProperty(nameof(dht_upload_rate_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int dht_upload_rate_limit { get; set; }

        [JsonProperty(nameof(disable_hash_checks), NullValueHandling = NullValueHandling.Ignore)]
        public bool disable_hash_checks { get; set; }

        [JsonProperty(nameof(disk_io_read_mode), NullValueHandling = NullValueHandling.Ignore)]
        public int disk_io_read_mode { get; set; }

        [JsonProperty(nameof(disk_io_write_mode), NullValueHandling = NullValueHandling.Ignore)]
        public int disk_io_write_mode { get; set; }

        [JsonProperty(nameof(disk_write_mode), NullValueHandling = NullValueHandling.Ignore)]
        public int disk_write_mode { get; set; }

        [JsonProperty(nameof(dont_count_slow_torrents), NullValueHandling = NullValueHandling.Ignore)]
        public bool dont_count_slow_torrents { get; set; }

        [JsonProperty(nameof(download_rate_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int download_rate_limit { get; set; }

        [JsonProperty(nameof(enable_dht), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_dht { get; set; }

        [JsonProperty(nameof(enable_incoming_tcp), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_incoming_tcp { get; set; }

        [JsonProperty(nameof(enable_incoming_utp), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_incoming_utp { get; set; }

        [JsonProperty(nameof(enable_ip_notifier), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_ip_notifier { get; set; }

        [JsonProperty(nameof(enable_lsd), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_lsd { get; set; }

        [JsonProperty(nameof(enable_natpmp), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_natpmp { get; set; }

        [JsonProperty(nameof(enable_outgoing_tcp), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_outgoing_tcp { get; set; }

        [JsonProperty(nameof(enable_outgoing_utp), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_outgoing_utp { get; set; }

        [JsonProperty(nameof(enable_set_file_valid_data), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_set_file_valid_data { get; set; }

        [JsonProperty(nameof(enable_upnp), NullValueHandling = NullValueHandling.Ignore)]
        public bool enable_upnp { get; set; }

        [JsonProperty(nameof(file_pool_size), NullValueHandling = NullValueHandling.Ignore)]
        public int file_pool_size { get; set; }

        [JsonProperty(nameof(handshake_client_version), NullValueHandling = NullValueHandling.Ignore)]
        public string handshake_client_version { get; set; }

        [JsonProperty(nameof(handshake_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int handshake_timeout { get; set; }

        [JsonProperty(nameof(hashing_threads), NullValueHandling = NullValueHandling.Ignore)]
        public int hashing_threads { get; set; }

        [JsonProperty(nameof(i2p_hostname), NullValueHandling = NullValueHandling.Ignore)]
        public string i2p_hostname { get; set; }

        [JsonProperty(nameof(i2p_inbound_length), NullValueHandling = NullValueHandling.Ignore)]
        public int i2p_inbound_length { get; set; }

        [JsonProperty(nameof(i2p_inbound_quantity), NullValueHandling = NullValueHandling.Ignore)]
        public int i2p_inbound_quantity { get; set; }

        [JsonProperty(nameof(i2p_outbound_length), NullValueHandling = NullValueHandling.Ignore)]
        public int i2p_outbound_length { get; set; }

        [JsonProperty(nameof(i2p_outbound_quantity), NullValueHandling = NullValueHandling.Ignore)]
        public int i2p_outbound_quantity { get; set; }

        [JsonProperty(nameof(i2p_port), NullValueHandling = NullValueHandling.Ignore)]
        public int i2p_port { get; set; }

        [JsonProperty(nameof(in_enc_policy), NullValueHandling = NullValueHandling.Ignore)]
        public int in_enc_policy { get; set; }

        [JsonProperty(nameof(inactive_down_rate), NullValueHandling = NullValueHandling.Ignore)]
        public int inactive_down_rate { get; set; }

        [JsonProperty(nameof(inactive_up_rate), NullValueHandling = NullValueHandling.Ignore)]
        public int inactive_up_rate { get; set; }

        [JsonProperty(nameof(inactivity_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int inactivity_timeout { get; set; }

        [JsonProperty(nameof(incoming_starts_queued_torrents), NullValueHandling = NullValueHandling.Ignore)]
        public bool incoming_starts_queued_torrents { get; set; }

        [JsonProperty(nameof(initial_picker_threshold), NullValueHandling = NullValueHandling.Ignore)]
        public int initial_picker_threshold { get; set; }

        [JsonProperty(nameof(listen_interfaces), NullValueHandling = NullValueHandling.Ignore)]
        public string listen_interfaces { get; set; }

        [JsonProperty(nameof(listen_queue_size), NullValueHandling = NullValueHandling.Ignore)]
        public int listen_queue_size { get; set; }

        [JsonProperty(nameof(listen_system_port_fallback), NullValueHandling = NullValueHandling.Ignore)]
        public bool listen_system_port_fallback { get; set; }

        [JsonProperty(nameof(local_service_announce_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int local_service_announce_interval { get; set; }

        [JsonProperty(nameof(max_allowed_in_request_queue), NullValueHandling = NullValueHandling.Ignore)]
        public int max_allowed_in_request_queue { get; set; }

        [JsonProperty(nameof(max_concurrent_http_announces), NullValueHandling = NullValueHandling.Ignore)]
        public int max_concurrent_http_announces { get; set; }

        [JsonProperty(nameof(max_failcount), NullValueHandling = NullValueHandling.Ignore)]
        public int max_failcount { get; set; }

        [JsonProperty(nameof(max_http_recv_buffer_size), NullValueHandling = NullValueHandling.Ignore)]
        public int max_http_recv_buffer_size { get; set; }

        [JsonProperty(nameof(max_metadata_size), NullValueHandling = NullValueHandling.Ignore)]
        public int max_metadata_size { get; set; }

        [JsonProperty(nameof(max_out_request_queue), NullValueHandling = NullValueHandling.Ignore)]
        public int max_out_request_queue { get; set; }

        [JsonProperty(nameof(max_paused_peerlist_size), NullValueHandling = NullValueHandling.Ignore)]
        public int max_paused_peerlist_size { get; set; }

        [JsonProperty(nameof(max_peer_recv_buffer_size), NullValueHandling = NullValueHandling.Ignore)]
        public int max_peer_recv_buffer_size { get; set; }

        [JsonProperty(nameof(max_peerlist_size), NullValueHandling = NullValueHandling.Ignore)]
        public int max_peerlist_size { get; set; }

        [JsonProperty(nameof(max_pex_peers), NullValueHandling = NullValueHandling.Ignore)]
        public int max_pex_peers { get; set; }

        [JsonProperty(nameof(max_piece_count), NullValueHandling = NullValueHandling.Ignore)]
        public int max_piece_count { get; set; }

        [JsonProperty(nameof(max_queued_disk_bytes), NullValueHandling = NullValueHandling.Ignore)]
        public int max_queued_disk_bytes { get; set; }

        [JsonProperty(nameof(max_rejects), NullValueHandling = NullValueHandling.Ignore)]
        public int max_rejects { get; set; }

        [JsonProperty(nameof(max_retry_port_bind), NullValueHandling = NullValueHandling.Ignore)]
        public int max_retry_port_bind { get; set; }

        [JsonProperty(nameof(max_suggest_pieces), NullValueHandling = NullValueHandling.Ignore)]
        public int max_suggest_pieces { get; set; }

        [JsonProperty(nameof(max_web_seed_connections), NullValueHandling = NullValueHandling.Ignore)]
        public int max_web_seed_connections { get; set; }

        [JsonProperty(nameof(metadata_token_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int metadata_token_limit { get; set; }

        [JsonProperty(nameof(min_announce_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int min_announce_interval { get; set; }

        [JsonProperty(nameof(min_reconnect_time), NullValueHandling = NullValueHandling.Ignore)]
        public int min_reconnect_time { get; set; }

        [JsonProperty(nameof(mixed_mode_algorithm), NullValueHandling = NullValueHandling.Ignore)]
        public int mixed_mode_algorithm { get; set; }

        [JsonProperty(nameof(mmap_file_size_cutoff), NullValueHandling = NullValueHandling.Ignore)]
        public int mmap_file_size_cutoff { get; set; }

        [JsonProperty(nameof(no_atime_storage), NullValueHandling = NullValueHandling.Ignore)]
        public bool no_atime_storage { get; set; }

        [JsonProperty(nameof(no_connect_privileged_ports), NullValueHandling = NullValueHandling.Ignore)]
        public bool no_connect_privileged_ports { get; set; }

        [JsonProperty(nameof(no_recheck_incomplete_resume), NullValueHandling = NullValueHandling.Ignore)]
        public bool no_recheck_incomplete_resume { get; set; }

        [JsonProperty(nameof(num_optimistic_unchoke_slots), NullValueHandling = NullValueHandling.Ignore)]
        public int num_optimistic_unchoke_slots { get; set; }

        [JsonProperty(nameof(num_outgoing_ports), NullValueHandling = NullValueHandling.Ignore)]
        public int num_outgoing_ports { get; set; }

        [JsonProperty(nameof(num_want), NullValueHandling = NullValueHandling.Ignore)]
        public int num_want { get; set; }

        [JsonProperty(nameof(optimistic_disk_retry), NullValueHandling = NullValueHandling.Ignore)]
        public int optimistic_disk_retry { get; set; }

        [JsonProperty(nameof(optimistic_unchoke_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int optimistic_unchoke_interval { get; set; }

        [JsonProperty(nameof(out_enc_policy), NullValueHandling = NullValueHandling.Ignore)]
        public int out_enc_policy { get; set; }

        [JsonProperty(nameof(outgoing_interfaces), NullValueHandling = NullValueHandling.Ignore)]
        public string outgoing_interfaces { get; set; }

        [JsonProperty(nameof(outgoing_port), NullValueHandling = NullValueHandling.Ignore)]
        public int outgoing_port { get; set; }

        [JsonProperty(nameof(peer_connect_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int peer_connect_timeout { get; set; }

        [JsonProperty(nameof(peer_dscp), NullValueHandling = NullValueHandling.Ignore)]
        public int peer_dscp { get; set; }

        [JsonProperty(nameof(peer_fingerprint), NullValueHandling = NullValueHandling.Ignore)]
        public string peer_fingerprint { get; set; }

        [JsonProperty(nameof(peer_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int peer_timeout { get; set; }

        [JsonProperty(nameof(peer_turnover), NullValueHandling = NullValueHandling.Ignore)]
        public int peer_turnover { get; set; }

        [JsonProperty(nameof(peer_turnover_cutoff), NullValueHandling = NullValueHandling.Ignore)]
        public int peer_turnover_cutoff { get; set; }

        [JsonProperty(nameof(peer_turnover_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int peer_turnover_interval { get; set; }

        [JsonProperty(nameof(piece_extent_affinity), NullValueHandling = NullValueHandling.Ignore)]
        public bool piece_extent_affinity { get; set; }

        [JsonProperty(nameof(piece_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int piece_timeout { get; set; }

        [JsonProperty(nameof(predictive_piece_announce), NullValueHandling = NullValueHandling.Ignore)]
        public int predictive_piece_announce { get; set; }

        [JsonProperty(nameof(prefer_rc4), NullValueHandling = NullValueHandling.Ignore)]
        public bool prefer_rc4 { get; set; }

        [JsonProperty(nameof(prefer_udp_trackers), NullValueHandling = NullValueHandling.Ignore)]
        public bool prefer_udp_trackers { get; set; }

        [JsonProperty(nameof(prioritize_partial_pieces), NullValueHandling = NullValueHandling.Ignore)]
        public bool prioritize_partial_pieces { get; set; }

        [JsonProperty(nameof(proxy_hostname), NullValueHandling = NullValueHandling.Ignore)]
        public string proxy_hostname { get; set; }

        [JsonProperty(nameof(proxy_hostnames), NullValueHandling = NullValueHandling.Ignore)]
        public bool proxy_hostnames { get; set; }

        [JsonProperty(nameof(proxy_password), NullValueHandling = NullValueHandling.Ignore)]
        public string proxy_password { get; set; }

        [JsonProperty(nameof(proxy_peer_connections), NullValueHandling = NullValueHandling.Ignore)]
        public bool proxy_peer_connections { get; set; }

        [JsonProperty(nameof(proxy_port), NullValueHandling = NullValueHandling.Ignore)]
        public int proxy_port { get; set; }

        [JsonProperty(nameof(proxy_tracker_connections), NullValueHandling = NullValueHandling.Ignore)]
        public bool proxy_tracker_connections { get; set; }

        [JsonProperty(nameof(proxy_type), NullValueHandling = NullValueHandling.Ignore)]
        public int proxy_type { get; set; }

        [JsonProperty(nameof(proxy_username), NullValueHandling = NullValueHandling.Ignore)]
        public string proxy_username { get; set; }

        [JsonProperty(nameof(rate_choker_initial_threshold), NullValueHandling = NullValueHandling.Ignore)]
        public int rate_choker_initial_threshold { get; set; }

        [JsonProperty(nameof(rate_limit_ip_overhead), NullValueHandling = NullValueHandling.Ignore)]
        public bool rate_limit_ip_overhead { get; set; }

        [JsonProperty(nameof(recv_socket_buffer_size), NullValueHandling = NullValueHandling.Ignore)]
        public int recv_socket_buffer_size { get; set; }

        [JsonProperty(nameof(report_redundant_bytes), NullValueHandling = NullValueHandling.Ignore)]
        public bool report_redundant_bytes { get; set; }

        [JsonProperty(nameof(report_true_downloaded), NullValueHandling = NullValueHandling.Ignore)]
        public bool report_true_downloaded { get; set; }

        [JsonProperty(nameof(report_web_seed_downloads), NullValueHandling = NullValueHandling.Ignore)]
        public bool report_web_seed_downloads { get; set; }

        [JsonProperty(nameof(request_queue_time), NullValueHandling = NullValueHandling.Ignore)]
        public int request_queue_time { get; set; }

        [JsonProperty(nameof(request_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int request_timeout { get; set; }

        [JsonProperty(nameof(resolver_cache_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int resolver_cache_timeout { get; set; }

        [JsonProperty(nameof(seed_choking_algorithm), NullValueHandling = NullValueHandling.Ignore)]
        public int seed_choking_algorithm { get; set; }

        [JsonProperty(nameof(seed_time_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int seed_time_limit { get; set; }

        [JsonProperty(nameof(seed_time_ratio_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int seed_time_ratio_limit { get; set; }

        [JsonProperty(nameof(seeding_outgoing_connections), NullValueHandling = NullValueHandling.Ignore)]
        public bool seeding_outgoing_connections { get; set; }

        [JsonProperty(nameof(seeding_piece_quota), NullValueHandling = NullValueHandling.Ignore)]
        public int seeding_piece_quota { get; set; }

        [JsonProperty(nameof(send_buffer_low_watermark), NullValueHandling = NullValueHandling.Ignore)]
        public int send_buffer_low_watermark { get; set; }

        [JsonProperty(nameof(send_buffer_watermark), NullValueHandling = NullValueHandling.Ignore)]
        public int send_buffer_watermark { get; set; }

        [JsonProperty(nameof(send_buffer_watermark_factor), NullValueHandling = NullValueHandling.Ignore)]
        public int send_buffer_watermark_factor { get; set; }

        [JsonProperty(nameof(send_not_sent_low_watermark), NullValueHandling = NullValueHandling.Ignore)]
        public int send_not_sent_low_watermark { get; set; }

        [JsonProperty(nameof(send_redundant_have), NullValueHandling = NullValueHandling.Ignore)]
        public bool send_redundant_have { get; set; }

        [JsonProperty(nameof(send_socket_buffer_size), NullValueHandling = NullValueHandling.Ignore)]
        public int send_socket_buffer_size { get; set; }

        [JsonProperty(nameof(share_mode_target), NullValueHandling = NullValueHandling.Ignore)]
        public int share_mode_target { get; set; }

        [JsonProperty(nameof(share_ratio_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int share_ratio_limit { get; set; }

        [JsonProperty(nameof(smooth_connects), NullValueHandling = NullValueHandling.Ignore)]
        public bool smooth_connects { get; set; }

        [JsonProperty(nameof(socks5_udp_send_local_ep), NullValueHandling = NullValueHandling.Ignore)]
        public bool socks5_udp_send_local_ep { get; set; }

        [JsonProperty(nameof(ssrf_mitigation), NullValueHandling = NullValueHandling.Ignore)]
        public bool ssrf_mitigation { get; set; }

        [JsonProperty(nameof(stop_tracker_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int stop_tracker_timeout { get; set; }

        [JsonProperty(nameof(strict_end_game_mode), NullValueHandling = NullValueHandling.Ignore)]
        public bool strict_end_game_mode { get; set; }

        [JsonProperty(nameof(suggest_mode), NullValueHandling = NullValueHandling.Ignore)]
        public int suggest_mode { get; set; }

        [JsonProperty(nameof(support_share_mode), NullValueHandling = NullValueHandling.Ignore)]
        public bool support_share_mode { get; set; }

        [JsonProperty(nameof(tick_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int tick_interval { get; set; }

        [JsonProperty(nameof(torrent_connect_boost), NullValueHandling = NullValueHandling.Ignore)]
        public int torrent_connect_boost { get; set; }

        [JsonProperty(nameof(tracker_backoff), NullValueHandling = NullValueHandling.Ignore)]
        public int tracker_backoff { get; set; }

        [JsonProperty(nameof(tracker_completion_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int tracker_completion_timeout { get; set; }

        [JsonProperty(nameof(tracker_maximum_response_length), NullValueHandling = NullValueHandling.Ignore)]
        public int tracker_maximum_response_length { get; set; }

        [JsonProperty(nameof(tracker_receive_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int tracker_receive_timeout { get; set; }

        [JsonProperty(nameof(udp_tracker_token_expiry), NullValueHandling = NullValueHandling.Ignore)]
        public int udp_tracker_token_expiry { get; set; }

        [JsonProperty(nameof(unchoke_interval), NullValueHandling = NullValueHandling.Ignore)]
        public int unchoke_interval { get; set; }

        [JsonProperty(nameof(unchoke_slots_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int unchoke_slots_limit { get; set; }

        [JsonProperty(nameof(upload_rate_limit), NullValueHandling = NullValueHandling.Ignore)]
        public int upload_rate_limit { get; set; }

        [JsonProperty(nameof(upnp_ignore_nonrouters), NullValueHandling = NullValueHandling.Ignore)]
        public bool upnp_ignore_nonrouters { get; set; }

        [JsonProperty(nameof(upnp_lease_duration), NullValueHandling = NullValueHandling.Ignore)]
        public int upnp_lease_duration { get; set; }

        [JsonProperty(nameof(urlseed_max_request_bytes), NullValueHandling = NullValueHandling.Ignore)]
        public int urlseed_max_request_bytes { get; set; }

        [JsonProperty(nameof(urlseed_pipeline_size), NullValueHandling = NullValueHandling.Ignore)]
        public int urlseed_pipeline_size { get; set; }

        [JsonProperty(nameof(urlseed_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int urlseed_timeout { get; set; }

        [JsonProperty(nameof(urlseed_wait_retry), NullValueHandling = NullValueHandling.Ignore)]
        public int urlseed_wait_retry { get; set; }

        [JsonProperty(nameof(use_dht_as_fallback), NullValueHandling = NullValueHandling.Ignore)]
        public bool use_dht_as_fallback { get; set; }

        [JsonProperty(nameof(use_parole_mode), NullValueHandling = NullValueHandling.Ignore)]
        public bool use_parole_mode { get; set; }

        [JsonProperty(nameof(user_agent), NullValueHandling = NullValueHandling.Ignore)]
        public string user_agent { get; set; }

        [JsonProperty(nameof(utp_connect_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_connect_timeout { get; set; }

        [JsonProperty(nameof(utp_cwnd_reduce_timer), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_cwnd_reduce_timer { get; set; }

        [JsonProperty(nameof(utp_fin_resends), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_fin_resends { get; set; }

        [JsonProperty(nameof(utp_gain_factor), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_gain_factor { get; set; }

        [JsonProperty(nameof(utp_loss_multiplier), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_loss_multiplier { get; set; }

        [JsonProperty(nameof(utp_min_timeout), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_min_timeout { get; set; }

        [JsonProperty(nameof(utp_num_resends), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_num_resends { get; set; }

        [JsonProperty(nameof(utp_syn_resends), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_syn_resends { get; set; }

        [JsonProperty(nameof(utp_target_delay), NullValueHandling = NullValueHandling.Ignore)]
        public int utp_target_delay { get; set; }

        [JsonProperty(nameof(validate_https_trackers), NullValueHandling = NullValueHandling.Ignore)]
        public bool validate_https_trackers { get; set; }

        [JsonProperty(nameof(web_seed_name_lookup_retry), NullValueHandling = NullValueHandling.Ignore)]
        public int web_seed_name_lookup_retry { get; set; }

        [JsonProperty(nameof(whole_pieces_threshold), NullValueHandling = NullValueHandling.Ignore)]
        public int whole_pieces_threshold { get; set; }
    }
}
