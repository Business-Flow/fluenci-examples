use std::thread;
use std::time::Duration;

fn main() {
    println!("Hello from the Deployment Script!");
    
    // Arbitrary delay to test the portal's automatic status refreshing.
    for i in (1..=15).rev() {
        println!("Pausing for {} seconds...", i);

        thread::sleep(Duration::from_secs(1));
    }
    println!("Deployment Script Completed.");
}
